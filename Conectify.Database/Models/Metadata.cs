namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.ComponentModel.DataAnnotations;

public class Metadata : Serializable, IEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public float? MinVal { get; set; }
    public float? MaxVal { get; set; }

    public Guid? ActuatorId { get; set; }
    public Guid? SensorId { get; set; }
    public Guid? DeviceId { get; set; }

    public virtual Actuator? Actuator { get; set; }

    public virtual Sensor? Sensor { get; set; }

    public virtual Device? Device { get; set; }
}
