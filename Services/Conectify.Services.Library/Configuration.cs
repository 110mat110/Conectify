using Microsoft.Extensions.Configuration;

namespace Conectify.Services.Library
{
    public class Configuration
    {
        public IConfigurationRoot Config { get; set; }
        public Configuration()
        {
            const string devSettings = "appsettings.Developement.json";
            const string prodSettings = "appsettings.json";
            var builder = new ConfigurationBuilder().AddJsonFile(File.Exists(devSettings) ? devSettings : prodSettings, optional: true, reloadOnChange: true);

            Config = builder.Build();

            Config.Bind(this);
        }

        public string WebsocketUrl { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;

        public Guid DeviceId { get; set; } = Guid.NewGuid();
    }
}