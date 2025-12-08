namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly1PMPro : Shelly
{
    public Shelly1PMPro()
    {

    }

    public Shelly1PMPro(string name, string id)
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
