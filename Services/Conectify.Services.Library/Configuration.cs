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
    }
}