namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Device : Serializable, IEntity, IMetadatable
{
    [Key]
    public Guid Id { get; set; }

    public string IPAdress { get; set; } = string.Empty;
    public string MacAdress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool SubscibeToAll { get; set; } = false;
    public bool IsKnown { get; set; } = false;
    public Guid? PositionId { get; set; }

    public virtual Position Position { get; set; } = null!;
    public virtual List<Metadata> Metadata { get; set; } = new List<Metadata>();
    public virtual IEnumerable<Sensor> Sensors { get; set; } = null!;
    public virtual IEnumerable<Actuator> Actuators { get; set; } = null!;

    public virtual IEnumerable<Preference> Preferences { get; set; } = new List<Preference>();
}
