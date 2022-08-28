using Conectify.Services.Library;

namespace Conectify.Services.Automatization
{
    public class AutomatizationConfiguration : Configuration
    {
        public string CurrentInstanceId { get; set; } = string.Empty;

        public int RefreshIntervalSeconds { get; set; } = 1;

        public string TargetIp { get; set; } = "Not bind";
    }
}
