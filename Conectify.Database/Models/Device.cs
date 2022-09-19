namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Device : Serializable, IEntity, IMetadatable<Device>, IDevice
{
    [Key]
    public Guid Id { get; set; }

    public string IPAdress { get; set; } = string.Empty;
    public string MacAdress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool SubscribeToAll { get; set; } = false;
    public bool IsKnown { get; set; } = false;
    public Guid? PositionId { get; set; }

    public virtual Position Position { get; set; } = null!;
    public virtual ICollection<MetadataConnector<Device>> Metadata { get; set; } = new List<MetadataConnector<Device>>();
    public virtual ICollection<Sensor> Sensors { get; set; } = null!;
    public virtual ICollection<Actuator> Actuators { get; set; } = null!;

    public virtual ICollection<Preference> Preferences { get; set; } = new HashSet<Preference>();
}
