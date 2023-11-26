using Conectify.Shared.Library.Interfaces;

namespace Conectify.Shared.Library.Models;
public record ApiFilter : IApiModel
{
    public bool IsVisible { get; set; }

    public string? Name { get; set; }
}
