namespace Conectify.Database.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;

public class Actuator : Serializable, IMetadatable<Actuator>, IEntity, IDevice
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
    public virtual ICollection<MetadataConnector<Actuator>> Metadata { get; set; } = new HashSet<MetadataConnector<Actuator>>();
}
