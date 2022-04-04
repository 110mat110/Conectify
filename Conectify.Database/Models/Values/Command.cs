namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public class Command : BaseInputType
{
    public Guid DestinationId { get; set; }
    public virtual Sensor Source { get; set; } = null!;
    public virtual Device Destination { get; set; } = null!;
}
