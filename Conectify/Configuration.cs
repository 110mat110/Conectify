namespace Conectify.Server;

public class Configuration
{
	public Configuration(IConfiguration configuration)
	{
		configuration.Bind(this);
	}

	public string HistoryService { get; set; } = string.Empty;
}
