using System.ComponentModel.DataAnnotations;

namespace Conectify.Database.Models.Updates;
public class DeviceVersion
{
    public Guid DeviceId { get; set; }

    public Guid SoftwareId { get; set; }

    public DateTime? LastUpdate { get; set; }

    public string ChipVersion { get; set; } = string.Empty;

    public virtual Device Device { get; set; } = null!;

    public virtual Software Software { get; set; } = null!;
}
