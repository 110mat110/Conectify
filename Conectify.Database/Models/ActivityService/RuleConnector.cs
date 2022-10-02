namespace Conectify.Database.Models.ActivityService;

public class RuleConnector
{
    public Guid ContinuingRuleId { get; set; }

    public Guid PreviousRuleId { get; set; }

    public virtual Rule ContinuingRule { get; set; } = null!;

    public virtual Rule PreviousRule { get; set; } = null!;
}
