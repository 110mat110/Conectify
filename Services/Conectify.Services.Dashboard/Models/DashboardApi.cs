namespace Conectify.Services.Dashboard.Models;

public record DashboardApi
{
    public Guid Id { get; set; }
    public string Background { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public IEnumerable<DashboardDeviceApi> DashboardDevices { get; set; } = new List<DashboardDeviceApi>();
}

public record DashboardDeviceApi
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public int PosX { get; set; }

    public int PosY { get; set; }

    public string SourceType { get; set; } = null!;
}
