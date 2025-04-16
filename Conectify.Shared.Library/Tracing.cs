using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Conectify.Shared.Library;
public class Tracing
{
    private static ActivityContext GetParentContext(Guid traceId)
    {
        string traceIdHex = traceId.ToString("N");
        ActivitySpanId spanId = ActivitySpanId.CreateRandom();
        string traceparent = $"00-{traceIdHex}-{spanId}-01";

        var parentContext = Activity.Current?.Context ?? default;
        if (Activity.Current?.Context.TraceId.ToHexString() != traceIdHex)
        {
            parentContext = ActivityContext.Parse(traceparent, "");
        }
        return parentContext;
    }

    private static void ExecuteWithTracing(Action action, Guid traceId, string activityName)
    {
        var source = new ActivitySource("CustomTracing");
        using var activity = source.StartActivity(activityName, ActivityKind.Internal, GetParentContext(traceId))?.SetTag("custom.deviceId", traceId);
        action.Invoke();
        activity?.Stop();
    }

    private static T ExecuteWithTracing<T>(Func<T> func, Guid traceId, string activityName)
    {
        var source = new ActivitySource("CustomTracing");
        using var activity = source.StartActivity(activityName, ActivityKind.Internal, GetParentContext(traceId))?.SetTag("custom.deviceId", traceId);
        var result = func.Invoke();
        activity?.Stop();
        return result;
    }

    public static void Trace(Action task, Guid traceId, string activityName)
        => ExecuteWithTracing(task, traceId == Guid.Empty ? Guid.NewGuid() : traceId, activityName);

    public static T Trace<T>(Func<T> task, Guid traceId, string activityName)
        => ExecuteWithTracing(task, traceId == Guid.Empty ? Guid.NewGuid() : traceId, activityName);

    public static async Task<T> Trace<T>(Func<Task<T>> task, Guid traceId, string activityName)
    {
        return await ExecuteWithTracing(async () => await task.Invoke(), traceId == Guid.Empty ? Guid.NewGuid() : traceId, activityName);
    }
}
