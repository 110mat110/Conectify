using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Models
{
    public class RuleDTO
    {
        private readonly Dictionary<Guid, AutomatisationValue> cachedValues = new Dictionary<Guid, AutomatisationValue>();

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ParametersJson { get; set; } = string.Empty;

        public Guid RuleTypeId { get; set; }

        public Guid? SourceSensorId { get; set; }

        public Guid? DestinationActuatorId { get; set; }

        public IEnumerable<Guid> NextRules { get; set; } = new List<Guid>();


        public void InsertValue(Value value)
        {
            var automationValue = new AutomatisationValue()
            {
                Id = value.Id,
                Name = value.Name,
                NumericValue = value.NumericValue,
                StringValue = value.StringValue,
                SourceId = value.SourceId,
                TimeCreated = value.TimeCreated,
                Unit = value.Unit,
            };

            InsertValue(automationValue);
        } 

        public void InsertValue(AutomatisationValue value)
        {
            if (cachedValues.ContainsKey(value.Id))
            {
                cachedValues[value.Id] = value;
            }
            else
            {
                cachedValues.Add(value.Id, value);
            }
        }

        public IEnumerable<AutomatisationValue> Values => cachedValues.Select(x => x.Value);
    }
}
