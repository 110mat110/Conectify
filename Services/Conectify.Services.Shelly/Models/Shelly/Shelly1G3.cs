
using Conectify.Database.Models;
using Conectify.Services.Shelly.Services;

namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly1G3 : IShelly
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

    public string Name { get; set; }
    public List<Switch> Switches { get; set;  } 
    public List<DetachedInput> DetachedInputs {get; set;} = [];
    public List<Power> Powers { get; set; }= [];

    public string Id { get; set; }
}
