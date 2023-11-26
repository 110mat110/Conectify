using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.Dashboard;
public class DashboardDevice : IEntity
{
    public Guid Id { get; set; }
    public Guid DashBoardId { get; set; }

    public Guid DeviceId { get; set; }

    public int PosX { get; set; }

    public int PosY { get; set; }

    public string SourceType { get; set; } = null!;
}
