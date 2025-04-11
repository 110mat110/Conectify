using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly2PMG3 : Shelly
{
    public Shelly2PMG3()
    {

    }

    public Shelly2PMG3(string name, string id)
    {
        Name = name;
        Id = id;
        Switches =
        [
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 0,
                Power = new(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0
                }
            },
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 1,
                Power = new(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 1
                }
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
            }
            ];
    }
}
