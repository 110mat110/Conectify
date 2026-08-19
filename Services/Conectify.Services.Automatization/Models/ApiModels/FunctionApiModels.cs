using Conectify.Services.Automatization.Models.Database;

namespace Conectify.Services.Automatization.Models.ApiModels;

public class GetFunctionApiModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? ParentFunctionId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    /// <summary>Ports the parent board connects to, in the order they were added.</summary>
    public IEnumerable<FunctionPortApiModel> InputPorts { get; set; } = [];

    public IEnumerable<FunctionPortApiModel> OutputPorts { get; set; } = [];
}

/// <summary>
/// One pin of a function. <see cref="PointId"/> is the point a connection is made to, so an input
/// port reports the input point of its rule and an output port the output point.
/// </summary>
public class FunctionPortApiModel
{
    public Guid RuleId { get; set; }

    public Guid PointId { get; set; }

    public string Name { get; set; } = string.Empty;

    public InputTypeEnum Type { get; set; }

    public int Index { get; set; }
}

public record CreateFunctionApiModel
{
    public string Name { get; set; } = string.Empty;

    public Guid? ParentFunctionId { get; set; }
}

public record EditFunctionApiModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int X { get; set; }

    public int Y { get; set; }
}

public enum FunctionPortDirection { Input, Output }

public record AddFunctionPortApiModel
{
    public FunctionPortDirection Direction { get; set; }

    /// <summary>Kind of signal the port carries, ignored for anything but the single input it creates.</summary>
    public InputTypeEnum Type { get; set; } = InputTypeEnum.Value;

    public string Name { get; set; } = string.Empty;
}

public record MoveToFunctionApiModel
{
    /// <summary>Function to move into, null moves the item onto the top level board.</summary>
    public Guid? FunctionId { get; set; }
}
