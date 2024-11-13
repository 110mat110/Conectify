namespace Conectify.Services.Shelly.Models.Shelly;

public class ShellyPmG3 : IShelly
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

    public string Name { get; set; }
    public List<Switch> Switches { get; set; }
    public List<DetachedInput> DetachedInputs { get; set; } = [];
    public List<Power> Powers { get; set; } = [];

    public string Id { get; set; }
}
