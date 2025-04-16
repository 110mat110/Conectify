using System.Collections.Generic;
using Conectify.Shared.Library.Interfaces;

namespace Conectify.Shared.Library.Models;
public record ApiFilter : IApiModel
{
    public bool IsVisible { get; set; }

    public string? Name { get; set; }

    public int Page { get; set; } = 0;

    public int Count { get; set; } = 20;

    public IEnumerable<ApiMetadataFilter> MetadataFilters { get; set; } = [];
}

public record ApiMetadataFilter : IApiModel
{
    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    public int? NumericValue { get; set; }

    public bool EqualityComparator { get; set; } = true;
}
