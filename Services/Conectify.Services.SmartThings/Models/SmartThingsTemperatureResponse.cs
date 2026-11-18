namespace Conectify.Services.SmartThings.Models;

public class SmartThingsTemperatureResponse
{
    public Temperature temperature { get; set; } = null!;
}

public class Temperature
{
    public int value { get; set; }
    public string unit { get; set; } = null!;
    public DateTimeOffset timestamp { get; set; }
}
