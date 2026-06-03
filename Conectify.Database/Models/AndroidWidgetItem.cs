namespace Conectify.Database.Models;

public class AndroidWidgetItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserMail { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;  // "large" | "small"
    public Guid DeviceId { get; set; }
    public string SourceType { get; set; } = string.Empty;  // "sensor" | "actuator"
    public int Position { get; set; }
}
