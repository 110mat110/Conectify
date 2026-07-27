using System.Diagnostics;

namespace Conectify.Services.Android.Services;

/// <summary>
/// Activity source for the widget path. Registered with the tracer provider in Program.cs, so every
/// span raised here shows up in Aspire nested under the incoming HTTP request.
/// </summary>
public static class AndroidTelemetry
{
    public const string SourceName = "Conectify.Android";

    public static readonly ActivitySource Source = new(SourceName);

    /// <summary>Tags the span that is currently in scope — usually the ASP.NET request span.</summary>
    public static void TagCurrent(string key, object? value) => Activity.Current?.SetTag(key, value);
}
