namespace Conectify.Services.Shelly.Models.Shelly;

public class ShellyI4 : Shelly
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
}
