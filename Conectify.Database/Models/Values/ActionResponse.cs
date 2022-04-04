namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public class ActionResponse : BaseInputType
{
    public Guid ActionId { get; }

    public virtual Device Source { get; set; } = null!;
    public virtual Action Action { get; set; } = null!;
}
