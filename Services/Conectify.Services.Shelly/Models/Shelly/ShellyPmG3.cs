namespace Conectify.Services.Shelly.Models.Shelly;

public class ShellyPmG3 : Shelly
{
    public ShellyPmG3()
    {

    }

    public ShellyPmG3(string name, string id)
    {
        Name = name;
        Id = id;
        Powers = [
            new Power(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0,
                },
            new Power(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 1,
                },];
        Switches = [];
        DetachedInputs = [];
    }
}
