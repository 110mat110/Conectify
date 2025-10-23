namespace Conectify.Services.SmartThings;

public class SmartThingsConfiguration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public required string ClientId {  get; set; }
    public required string ClientSecret { get; set; }
}
