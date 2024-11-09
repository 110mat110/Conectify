using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public class ShellyI4 : IShelly
{
    public ShellyI4()
    {

    }

    public ShellyI4(string name, string id)
    {
        Name = name;
        Id = id;
        Switches = [];
        DetachedInputs =
        [
            new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 0,
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 0,
                }
            },
            new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 1,
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 1,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 1,
                }
            },
                        new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 2,
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 2,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 2,
                }
            },
                                    new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 3,
                DoublePress = new DoublePress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 3,
                },
                LongPress = new LongPress(){
                    SensorId = Guid.NewGuid(),
                    ShellyId= 3,
                }
            }

        ];
    }

    public ShellyType ShellyType { get; set; } = ShellyType.Shelly1g3;

    public string Name { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; } = [];
    public List<Power> Powers { get; set; } = [];

    public string Id { get; set; }

}
