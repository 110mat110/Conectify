namespace Conectify.Services.Automatization.Models.Database;

public class RuleConnector
{
    public Guid SourceRuleId { get; set; }

    public Guid TargetRuleId { get; set; }

    public virtual OutputPoint SourceRule { get; set; } = null!;

    public virtual InputPoint TargetRule { get; set; } = null!;
}
