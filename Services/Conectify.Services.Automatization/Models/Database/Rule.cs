namespace Conectify.Services.Automatization.Models.Database;

public class Rule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = string.Empty;

    public Guid RuleType { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public virtual IEnumerable<OutputPoint> OutputConnectors { get; set; } = null!;

    public virtual IEnumerable<InputPoint> InputConnectors { get; set; } = null!;
}
