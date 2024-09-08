using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.Updates;
public class SoftwareVersion : IEntity
{
    public Guid Id { get; set; }

    public Guid SoftwareId { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string Url { get; set; } = string.Empty;

    public virtual Software Software { get; set; } = null!;
}
