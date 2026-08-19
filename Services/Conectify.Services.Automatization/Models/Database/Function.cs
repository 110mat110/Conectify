namespace Conectify.Services.Automatization.Models.Database;

/// <summary>
/// A group of rules with its own inputs and outputs. Functions can be nested, which makes a big
/// automatization readable: the parent board shows one box instead of a dozen rules.
/// </summary>
public class Function
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Function this one lives in, null when it sits on the top level board.</summary>
    public Guid? ParentFunctionId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public virtual Function? ParentFunction { get; set; }

    public virtual IEnumerable<Function> Functions { get; set; } = null!;

    public virtual IEnumerable<Rule> Rules { get; set; } = null!;
}
