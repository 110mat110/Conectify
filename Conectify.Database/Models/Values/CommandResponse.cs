namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public class CommandResponse : BaseInputType
{
    public Guid CommandId { get; }

    public virtual Device Source { get; set; } = null!;
    public virtual Command Command { get; set; } = null!;
}
