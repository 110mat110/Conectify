using Conectify.Shared.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Library
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection UseConectifyWebsocket<TConfiguration, TDeviceData>(this IServiceCollection services) where TConfiguration : Configuration where TDeviceData : class, IDeviceData
        {
            services.AddAutoMapper(typeof(TDeviceData).Assembly, typeof(DeviceProfile).Assembly);
            services.AddSingleton<IServicesWebsocketClient, ServicesWebsocketClient>();
            services.AddTransient<TConfiguration>();
            services.AddTransient<Configuration, TConfiguration>();
            services.AddTransient<IConnectorService, ConnectorService>();
            services.AddTransient<IDeviceData, TDeviceData>();

            return services;
        }

        public static async Task ConnectToConectifyServer(this IServiceProvider serviceProvider)
        {
            var connector = serviceProvider.GetRequiredService<IConnectorService>();
            var deviceData = serviceProvider.GetRequiredService<IDeviceData>();
            await connector.RegisterDevice(deviceData.ApiDevice, deviceData.ApiSensors, deviceData.ApiActuators);

            var ws = serviceProvider.GetRequiredService<IServicesWebsocketClient>();
            await ws.ConnectAsync();
        }
    }
}
