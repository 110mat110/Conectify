namespace Conectify.Database.Models.Values;

using Conectify.Database.Interfaces;
using System;

public class Action : BaseInputType
{
    public Guid? DestinationId { get; set; }
    public virtual Sensor Source { get; set; } = null!;
    public virtual Actuator Destination { get; set; } = null!;

}
