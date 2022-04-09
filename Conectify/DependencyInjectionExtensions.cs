namespace Conectify.Server;

using Conectify.Server.Caches;
using Conectify.Server.Services;

public static class DependencyInjectionExtensions
{
    public static void AddConectifyServices(this IServiceCollection services)
    {
        services.AddSingleton<IWebSocketService, WebSocketService>();
        services.AddTransient<IActuatorService, ActuatorService>();
        services.AddTransient<ISensorService, SensorService>();
        services.AddTransient<IDataService, DataService>();
        services.AddTransient<IDeviceService, DeviceService>();
        services.AddTransient<IPipelineService, PipelineService>();
        services.AddSingleton<ISubscribersCache,SubscribersCache>();
    }
}
