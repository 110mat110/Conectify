namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;

public record Value : BaseInputType
{
    public virtual Sensor Source { get; set; } = null!;
}
