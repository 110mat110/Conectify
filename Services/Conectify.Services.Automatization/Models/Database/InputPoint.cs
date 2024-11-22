namespace Conectify.Services.Automatization.Models.Database;

public enum InputTypeEnum { Parameter, Trigger, Value }


public class InputPoint
{
    public Guid Id { get; set; }

    public Guid RuleId { get; set; }

    public InputTypeEnum Type { get; set; }

    public int Index { get; set; }

    public virtual IEnumerable<RuleConnector> PreviousRules { get; set; } = null!;
    public virtual Rule Rule { get; set; } = null!;
}
