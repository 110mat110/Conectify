namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public record CommandResponse : BaseInputType
{
    public Guid CommandId { get; set; }

    public virtual Device Source { get; set; } = null!;
    public virtual Command Command { get; set; } = null!;
}
