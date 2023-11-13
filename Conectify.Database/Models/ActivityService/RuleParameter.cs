namespace Conectify.Database.Models.ActivityService;

public class RuleParameter
{
        public Guid SourceRuleId { get; set; }

        public Guid TargetRuleId { get; set; }

        public virtual Rule SourceRule { get; set; } = null!;

        public virtual Rule TargetRule { get; set; } = null!;
}
