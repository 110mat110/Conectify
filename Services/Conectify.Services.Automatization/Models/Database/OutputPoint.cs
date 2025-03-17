namespace Conectify.Services.Automatization.Models.Database;

public class OutputPoint
{
    public Guid Id { get; set; }

    public Guid RuleId { get; set; }

    public int Index { get; set; }

    public virtual IEnumerable<RuleConnector> ContinousRules { get; set; } = null!;
    public virtual Rule Rule { get; set; } = null!;
}
