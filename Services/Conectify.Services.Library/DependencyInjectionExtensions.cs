using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Library
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection UseConectifyWebsocket<TConfiguration>(this IServiceCollection services) where TConfiguration : class
        {
            services.AddSingleton<IServicesWebsocketClient, ServicesWebsocketClient>();
            services.AddTransient<TConfiguration>();

            return services;
        }
    }
}
