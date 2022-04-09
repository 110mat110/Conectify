namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public record Command : BaseInputType
{
    public Guid DestinationId { get; set; }
    public virtual Device Source { get; set; } = null!;
    public virtual Device Destination { get; set; } = null!;
}
