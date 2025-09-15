namespace Conectify.Services.Shelly.Models.Shelly;

public class Shelly3EM : Shelly
{
    public Shelly3EM()
    {

    }

    public Shelly3EM(string name, string id)
    {
        Name = name;
        Id = id;
        Powers = [
            new Power(){
                    SensorId = Guid.NewGuid(),
                    ShellyId = 0,
                }];
        Switches = [];
        DetachedInputs = [];
    }
}
