namespace Conectify.Services.SmartThings.Models;

public class SmartThingsDeviceResponse
{
    public List<SmartThingsDevice> Items { get; set; } = [];
}

public class SmartThingsDevice
{
    public Guid DeviceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public string DeviceTypeName { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public List<Component> Components { get; set; } = [];
}

public class Component
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<Capability> Capabilities { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}

public class Capability
{
    public string Id { get; set; } = string.Empty;
    public int Version { get; set; }
}

public class Category
{
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
}
