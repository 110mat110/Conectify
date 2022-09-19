using Conectify.Shared.Library.Interfaces;
using System;
using System.Collections.Generic;

namespace Conectify.Shared.Library.Models
{
    public record ApiPreference : IApiModel
    {
        public Guid? ActuatorId { get; set; }
        public Guid? SensorId { get; set; }
        public Guid? DeviceId { get; set; }

        public bool SubToValues { get; set; } = false;
        public bool SubToActions { get; set; } = false;
        public bool SubToCommands { get; set; } = false;
        public bool SubToActionResponse { get; set; } = false;
        public bool SubToCommandResponse { get; set; } = false;
    }

    public record ApiPreferences : IApiModel
    {
        public IEnumerable<ApiPreference> Preferences { get; set; } = new List<ApiPreference>();
    }
}
