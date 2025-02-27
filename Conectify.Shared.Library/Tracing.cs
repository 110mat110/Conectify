using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Conectify.Shared.Library;
public class Tracing
{
    public static void Trace(Action task, Guid traceId, string activityName)
    {
        if (traceId == Guid.Empty) traceId = Guid.NewGuid();

        string traceIdHex = traceId.ToString("N"); // Remove dashes, 32-char hex

        ActivitySpanId spanId = ActivitySpanId.CreateRandom();
        string traceparent = $"00-{traceIdHex}-{spanId}-01";

        var source = new ActivitySource("CustomTracing");

        var parentContext = Activity.Current?.Context ?? ActivityContext.Parse(traceparent, "");


        if (source is null)
        {
            task.Invoke();
            return;
        }
        using var activity = source.StartActivity(activityName, ActivityKind.Internal, parentContext)?.SetTag("custom.deviceId", traceId);
        task.Invoke();
        activity?.Stop();
    }

    public static T Trace<T>(Func<T> task,  Guid traceId, string activityName)
    {
        if (traceId == Guid.Empty) traceId = Guid.NewGuid();

        string traceIdHex = traceId.ToString("N"); // Remove dashes, 32-char hex

        ActivitySpanId spanId = ActivitySpanId.CreateRandom();
        string traceparent = $"00-{traceIdHex}-{spanId}-01";

        var source = new ActivitySource("CustomTracing");

        var parentContext = Activity.Current?.Context ?? ActivityContext.Parse(traceparent, "");

        if (source is null)
        {
            return task.Invoke();
        }
        using var activity = source.StartActivity(activityName, ActivityKind.Internal, parentContext)?.SetTag("custom.deviceId", traceId);
        var res = task.Invoke();
        activity?.Stop();

        return res;
    }

    public static async Task<T> Trace<T>(Func<Task<T>> task, Guid traceId, string activityName)
    {
        if (traceId == Guid.Empty) traceId = Guid.NewGuid();

        string traceIdHex = traceId.ToString("N"); // Remove dashes, 32-char hex

        ActivitySpanId spanId = ActivitySpanId.CreateRandom();
        string traceparent = $"00-{traceIdHex}-{spanId}-01";

        var source = new ActivitySource("CustomTracing");

        var parentContext = Activity.Current?.Context ?? ActivityContext.Parse(traceparent, "");


        if (source is null)
        {
            return await task.Invoke();
        }
        using var activity = source.StartActivity(activityName, ActivityKind.Internal, parentContext)?.SetTag("custom.deviceId", traceId);
        var res =  await task.Invoke();
        activity?.Stop();
        return res;
    }
}
