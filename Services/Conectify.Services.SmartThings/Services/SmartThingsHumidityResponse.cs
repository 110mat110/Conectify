namespace Conectify.Services.SmartThings.Services;

public class SmartThingsHumidityResponse
{
    public Humidity humidity { get; set; }
}

public class Humidity
{
    public int value { get; set; }
    public string unit { get; set; }
    public DateTimeOffset timestamp { get; set; }
}
