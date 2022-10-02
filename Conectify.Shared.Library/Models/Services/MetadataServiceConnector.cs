using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Shared.Library.Models.Services;

public record MetadataServiceConnector
{
    public float? NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public int? TypeValue { get; set; }
    public string Unit { get; set; } = string.Empty;

    public float? MinVal { get; set; }
    public float? MaxVal { get; set; }

    public string MetadataName { get; set; } = string.Empty;
}
