namespace Conectify.Services.MQTTTasker.Models;

public class IkeaDoorSensorValue
{
    public int battery { get; set; }
    public bool contact { get; set; }
    public int linkquality { get; set; }
    public Update update { get; set; }
    public int voltage { get; set; }
}

public class Update
{
    public int installed_version { get; set; }
    public int latest_version { get; set; }
    public string state { get; set; }
}