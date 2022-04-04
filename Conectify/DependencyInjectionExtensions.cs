namespace Conectify.Server;

using Conectify.Server.Services;

public static class DependencyInjectionExtensions
{
    public static void AddIoTHomeServices(this IServiceCollection services)
    {
        services.AddSingleton<IWebSocketService, WebSocketService>();
    }
}
