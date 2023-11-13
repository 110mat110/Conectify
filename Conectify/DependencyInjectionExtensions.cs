namespace Conectify.Server;

using Conectify.Server.Caches;
using Conectify.Server.Services;
using Conectify.Shared.Library.Services;

public static class DependencyInjectionExtensions
{
    public static void AddConectifyServices(this IServiceCollection services)
    {
        services.AddScoped<IWebSocketService, WebSocketService>();
        services.AddTransient<IActuatorService, ActuatorService>();
        services.AddTransient<ISensorService, SensorService>();
        services.AddTransient<IDataService, DataService>();
        services.AddTransient<IDeviceService, DeviceService>();
        services.AddTransient<IPipelineService, PipelineService>();
        services.AddSingleton<ISubscribersCache, SubscribersCache>();
        services.AddSingleton<IWebsocketCache, WebsocketCache>();
        services.AddTransient<IMetadataService, MetadataService>();
		services.AddSingleton<Configuration>();
		services.AddTransient<IHttpFactory, HttpFactory>();
        services.AddSingleton<IDeviceStatusService, DeviceStatusService>();
	}
}
