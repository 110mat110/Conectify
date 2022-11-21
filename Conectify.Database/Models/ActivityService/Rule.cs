using Conectify.Database.Interfaces;

namespace Conectify.Database.Models.ActivityService;

public class Rule : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = string.Empty;

    public Guid RuleType { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public virtual IEnumerable<RuleConnector> ContinuingRules { get; set; } = null!;

    public virtual IEnumerable<RuleConnector> PreviousRules { get; set; } = null!;
}
