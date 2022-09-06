using Microsoft.Extensions.Configuration;

namespace Conectify.Services.Library
{
    public class Configuration
    {
        public IConfigurationRoot Config { get; set; }
        public Configuration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Config = builder.Build();

            Config.Bind(this);
        }

        public string WebsocketUrl { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;

        public Guid DeviceId { get; set; } = Guid.NewGuid();
    }
}