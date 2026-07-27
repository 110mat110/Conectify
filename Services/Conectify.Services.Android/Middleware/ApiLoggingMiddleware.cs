using System.Diagnostics;
using System.Text;
using Conectify.Services.Android.Services;

namespace Conectify.Services.Android.Middleware;

/// <summary>
/// Logs every call the phone makes: the request line, its body, the status code, how long it took
/// and what went back. Scoped to the widget API so static files and the config page do not drown it.
///
/// Each line carries the trace id, so a log entry in Aspire links straight to the trace that
/// produced it, and the same facts are also written onto the request span as tags.
/// </summary>
public class ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
{
    private const string ApiPrefix = "/api/android";
    private const int MaxLoggedBodyChars = 2000;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var request = context.Request;
        var traceId = Activity.Current?.TraceId.ToString() ?? "none";
        var requestBody = await ReadRequestBodyAsync(request);

        logger.LogInformation(
            "→ {Method} {Path}{Query} from {ClientIp} [trace {TraceId}] body={RequestBody}",
            request.Method,
            request.Path.Value,
            request.QueryString.Value,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            traceId,
            Truncate(requestBody));

        AndroidTelemetry.TagCurrent("conectify.client_ip", context.Connection.RemoteIpAddress?.ToString());
        if (requestBody.Length > 0)
        {
            AndroidTelemetry.TagCurrent("conectify.request_body", Truncate(requestBody));
        }

        // Buffer the response so we can report its size and content. These payloads are a few KB,
        // so holding one in memory is cheaper than not being able to see what the phone received.
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "✗ {Method} {Path} threw after {ElapsedMs}ms [trace {TraceId}]",
                request.Method, request.Path.Value, stopwatch.ElapsedMilliseconds, traceId);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            buffer.Position = 0;
            var responseBody = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync();
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var status = context.Response.StatusCode;
            AndroidTelemetry.TagCurrent("conectify.duration_ms", stopwatch.ElapsedMilliseconds);
            AndroidTelemetry.TagCurrent("conectify.response_bytes", buffer.Length);

            // Anything non-2xx is worth a warning with the body attached — that is the case where
            // the phone shows an error and you need to know why.
            if (status >= 400)
            {
                logger.LogWarning(
                    "← {StatusCode} {Method} {Path} in {ElapsedMs}ms, {ResponseBytes} bytes [trace {TraceId}] body={ResponseBody}",
                    status, request.Method, request.Path.Value, stopwatch.ElapsedMilliseconds,
                    buffer.Length, traceId, Truncate(responseBody));
            }
            else
            {
                logger.LogInformation(
                    "← {StatusCode} {Method} {Path} in {ElapsedMs}ms, {ResponseBytes} bytes [trace {TraceId}]",
                    status, request.Method, request.Path.Value, stopwatch.ElapsedMilliseconds,
                    buffer.Length, traceId);

                // The full payload only at Debug, so Information stays readable at one line per call.
                logger.LogDebug("← body {ResponseBody}", Truncate(responseBody));
            }
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string Truncate(string value) =>
        value.Length <= MaxLoggedBodyChars ? value : value[..MaxLoggedBodyChars] + "…";
}
