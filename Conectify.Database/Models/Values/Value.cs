namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;

[Serializable()]
public class Value : BaseInputType
{
    public virtual Sensor Source { get; set; } = null!;
}
