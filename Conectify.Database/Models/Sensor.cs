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
    public string Name { get; set; } = string.Empty;
    public Guid SourceDeviceId { get; set; }
    public bool IsKnown { get; set; } = false;


    public virtual Device SourceDevice { get; set; } = null!;
    public virtual List<Metadata> Metadata { get; set; } = new List<Metadata>();
}
