namespace Conectify.Server.Caches;
using System;
using System.Collections.Generic;
using Conectify.Database.Models;

public record Subscriber(Guid DeviceId, IEnumerable<Preference> Preferences, bool IsSubedToAll, IEnumerable<Guid> Actuators, IEnumerable<Guid> Sensors);
