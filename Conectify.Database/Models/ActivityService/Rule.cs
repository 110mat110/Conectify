using Conectify.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Database.Models.ActivityService
{
    public class Rule : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ParametersJson { get; set; } = string.Empty;

        public Guid RuleType { get; set; }

        public Guid? SourceSensorId { get; set; }

        public Guid? DestinationActuatorId { get; set; }

        public virtual Sensor? SourceSensor { get; set; }

        public virtual Actuator? DestinationActuator { get; set; }

        public virtual IEnumerable<RuleConnector> ContinuingRules { get; set; } = null!;

        public virtual IEnumerable<RuleConnector> PreviousRules { get; set; } = null!;
    }
}
