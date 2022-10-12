using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Conectify.Shared.Maps;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Conectify.Services.Library;

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
        services.AddTransient<IHttpFactory, HttpFactory>();
        return services;
    }

    public static async Task ConnectToConectifyServer(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        var connector = serviceProvider.GetRequiredService<IConnectorService>();
        var deviceData = serviceProvider.GetRequiredService<IDeviceData>();
        await connector.RegisterDevice(deviceData.Device, deviceData.Sensors, deviceData.Actuators, ct);
        await connector.SetPreferences(deviceData.Device.Id, deviceData.Preferences, ct);
        await connector.SendMetadataForDevice(deviceData.Device.Id, deviceData.MetadataConnectors, ct);
        var ws = serviceProvider.GetRequiredService<IServicesWebsocketClient>();
        await ws.ConnectAsync();
    }
}
