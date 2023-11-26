using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.Dashboard;
public class Dashboard : IEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public int Position { get; set; }

    public string Background { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DashboardType Type { get; set; } = DashboardType.Basic;

    public virtual User User { get; set; } = null!;
}

public enum DashboardType { Basic, Grid}
