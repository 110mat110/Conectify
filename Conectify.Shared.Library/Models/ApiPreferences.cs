using Conectify.Shared.Library.Interfaces;
using System;
using System.Collections.Generic;

namespace Conectify.Shared.Library.Models;

public record ApiPreference : IApiModel
{
    public Guid? SubscibeeId { get; set; }

    public string EventType { get; set; } = string.Empty;
}

public record ApiPreferences : IApiModel
{
    public IEnumerable<ApiPreference> Preferences { get; set; } = [];
}
