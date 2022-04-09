namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Actuator : Serializable, IMetadatable, IEntity
{
    [Key]
    public Guid Id { get; set; }

    public static string Type = "Actuator";
    public string Name { get; set; } = string.Empty;
    public Guid SourceDeviceId { get; set; }
    public Guid SensorId { get; set; }
    public bool IsKnown { get; set; } = false;


    public virtual Device SourceDevice { get; set; } = null!;
    public virtual Sensor Sensor { get; set; } = null!;
    public virtual List<Metadata> Metadata { get; set; } = new List<Metadata>();

}
