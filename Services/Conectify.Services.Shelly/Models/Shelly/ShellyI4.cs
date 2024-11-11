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
            },
                                    new DetachedInput()
            {
                SensorId = Guid.NewGuid(),
                ShellyId = 3,
            }

        ];
    }
    public string Name { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; } = [];
    public List<Power> Powers { get; set; } = [];

    public string Id { get; set; }

}
