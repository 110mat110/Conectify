using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;

namespace Conectify.Services.Automatization.Models;

public class RuleDTO
{
    private readonly Dictionary<Guid, AutomatisationValue> cachedValues = new();

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = string.Empty;

    public Guid RuleTypeId { get; set; }

    public Guid? SourceSensorId { get; set; }

    public Guid? DestinationActuatorId { get; set; }

    public IEnumerable<Guid> NextRules { get; set; } = new List<Guid>();

    public IEnumerable<Guid> Parameters { get; set; } = new List<Guid>();

    public AutomatisationValue? OutputValue { get; set; }

    public void InsertEvent(Event evnt)
    {
        var automationValue = new AutomatisationValue()
        {
            Id = evnt.Id,
            Type = evnt.Type,
            Name = evnt.Name,
            NumericValue = evnt.NumericValue,
            StringValue = evnt.StringValue,
            SourceId = evnt.SourceId,
            TimeCreated = evnt.TimeCreated,
            Unit = evnt.Unit,
        };

        InsertValue(automationValue);
    }

    public void InsertValue(AutomatisationValue value)
    {
        if (cachedValues.ContainsKey(value.SourceId))
        {
            cachedValues[value.SourceId] = value;
        }
        else
        {
            cachedValues.Add(value.SourceId, value);
        }
    }

    public IEnumerable<AutomatisationValue> Values => cachedValues.Select(x => x.Value);

    public void Initialize()
    {
       var behaviour = BehaviourFactory.GetRuleBehaviorByTypeId(RuleTypeId);
       this.OutputValue = behaviour?.InitializationValue(this);
    }
}
