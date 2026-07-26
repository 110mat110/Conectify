namespace Conectify.Services.UI.Models;

/// <summary>The (name, value) pair a cube needs to look up its metadata icon on the client.</summary>
public record UiIcon
{
    public string Name { get; init; } = string.Empty;
    public string StringValue { get; init; } = string.Empty;
}
