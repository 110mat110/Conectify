namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Sensor : Serializable, IMetadatable, IEntity
{
    [Key]
    public Guid Id { get; set; }
    public static string Type = "Sensor";
    public string SensorName { get; set; } = string.Empty;
    public virtual List<Metadata> Metadata { get; set; } = new List<Metadata>();

    public Guid SourceDeviceId { get; set; }

    public virtual Device SourceDevice { get; set; } = null!;
}
