namespace Conectify.Server.Caches;
using Conectify.Database.Models;
using System;
using System.Collections.Generic;

public record Subscriber()
{
    public Guid DeviceId { get; set; }
    public IEnumerable<Preference> Preferences { get; set; } = new List<Preference>();
    public bool IsSubedToAll { get; set; }
    public IEnumerable<Guid> AllDependantIds { get; set; } = Enumerable.Empty<Guid>();
}
