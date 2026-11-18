namespace Conectify.Services.SmartThings.Services;

public class SmartThingsSwitchResponse
{
    public SwitchState @switch { get; set; } = null!;
}

public class SwitchState
{
    public string value { get; set; } = null!;
    public DateTimeOffset timestamp { get; set; }
}
