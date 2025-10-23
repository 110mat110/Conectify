namespace Conectify.Services.SmartThings.Services;

public class SmartThingsSwitchResponse
{
    public SwitchState @switch { get; set; }
}

public class SwitchState
{
    public string value { get; set; }
    public DateTimeOffset timestamp { get; set; }
}
