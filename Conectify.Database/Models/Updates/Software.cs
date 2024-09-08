using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.Updates;
public class Software : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public virtual ICollection<SoftwareVersion> Versions { get; set; } = new HashSet<SoftwareVersion>();
}
