namespace Conectify.Server.Caches;
using System;
using System.Collections.Generic;
using Conectify.Database.Models;

public record Subscriber()
{
    public Guid DeviceId { get; set; }
    public IEnumerable<Preference> Preferences { get; set; } = [];
    public bool IsSubedToAll { get; set; }
    public IEnumerable<Guid> AllDependantIds { get; set; } = Enumerable.Empty<Guid>();
}
