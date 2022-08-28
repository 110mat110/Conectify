using Conectify.Services.Library;

namespace TestService
{
    public class LocalConfig : Configuration
    {
        public string TargetIp { get; set; } = "Not bind";
    }
}
