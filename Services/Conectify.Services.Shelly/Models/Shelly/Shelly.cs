using Conectify.Database.Models;
using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public interface IShelly
{
    public ShellyType ShellyType { get; }
    public string Name { get; set; }
    public string Id { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; }
    public List<Power> Powers { get; set; }

}

public class Switch
{
    public int ShellyId { get; set; }

    public Guid ActuatorId { get; set; }

    public Guid SensorId { get; set; }

    public LongPress LongPress { get; set; }
    public DoublePress DoublePress { get; set; }

    public Power? Power { get; set; }
}

public class LongPress
{
    public int ShellyId { get; set; }

    public Guid SensorId { get; set; }
}

public class DoublePress
{
    public int ShellyId { get; set; }

    public Guid SensorId { get; set; }

}

public class DetachedInput
{
    public int ShellyId { get; set; }
    public Guid SensorId { get; set; }

    public LongPress LongPress { get; set; }
    public DoublePress DoublePress { get; set; }
}

public class Power
{
    public int ShellyId { get; set; }
    public Guid SensorId { get; set; }
}