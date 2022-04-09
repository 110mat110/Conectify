namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public record ActionResponse : BaseInputType
{
    public Guid ActionId { get; }

    public virtual Actuator Source { get; set; } = null!;
    public virtual Action Action { get; set; } = null!;
}
