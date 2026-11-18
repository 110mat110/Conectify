namespace Conectify.Services.SmartThings.Services;

public class SmartThingsHumidityResponse
{
    public Humidity humidity { get; set; } = null!;
}

public class Humidity
{
    public int value { get; set; }
    public string unit { get; set; } = null!;
    public DateTimeOffset timestamp { get; set; }
}
