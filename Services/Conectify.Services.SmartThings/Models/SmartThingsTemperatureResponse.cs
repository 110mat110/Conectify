namespace Conectify.Services.SmartThings.Models;

public class SmartThingsTemperatureResponse
{
    public Temperature temperature { get; set; }
}

public class Temperature
{
    public int value { get; set; }
    public string unit { get; set; }
    public DateTimeOffset timestamp { get; set; }
}
