using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly2PMG3 : IShelly
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
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 0,
                },
                Power = new(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0
                }
            },
            new Switch() {
                SensorId = Guid.NewGuid(),
                ActuatorId = Guid.NewGuid(),
                ShellyId = 1,
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 1,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 1,
                },
                Power = new(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 1
                }
            }
        ];
    }

    public ShellyType ShellyType { get; set; } = ShellyType.Shelly2pmg3;

    public string Name { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; } = [];
    public List<Power> Powers { get; set; } = [];

    public string Id { get; set; }
}
