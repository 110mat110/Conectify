namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly1G3 : Shelly
{
    public Shelly1G3()
    {

    }

    public Shelly1G3(string name, string id)
    {
        Name = name;
        Id = id;
        Switches =
        [
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 0
            }
        ];
        DetachedInputs = [
            new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 0
            }
            ];
    }
}
