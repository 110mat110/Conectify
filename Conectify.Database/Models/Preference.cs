namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;

public record Preference : IEntity
{
    public Guid Id { get; set; }
    public Guid SubscriberId { get; set; }
    
    public Guid? ActuatorId { get; set; }
    public Guid? SensorId { get; set; }
    public Guid? DeviceId { get; set; }

    public bool SubToValues { get; set; } = false;
    public bool SubToActions { get; set; } = false;
    public bool SubToCommands { get; set; } = false;
    public bool SubToActionResponse { get; set; } = false;
    public bool SubToCommandResponse { get; set; } = false;

    public virtual Sensor? Sensor { get; set; }
    public virtual Actuator? Actuator { get; set; }
    public virtual Device? Device { get; set; }
}