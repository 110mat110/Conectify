namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly3Pro : Shelly
{
    public Shelly3Pro()
    {

    }

    public Shelly3Pro(string name, string id)
    {
        Name = name;
        Id = id;
        Switches =
        [
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 0,
            },
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 1,
            },
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 2,
            }
        ];
        DetachedInputs =
[
    new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 0,
            },
            new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 1,
            },
                        new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 2,
            }
            ];
    }
}
