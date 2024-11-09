namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;

public record Preference : IEntity
{
    public Guid Id { get; set; }
    public Guid SubscriberId { get; set; }

    public Guid? SubscibeeId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public virtual Device Subscriber { get; set; } = null!;
}