namespace Conectify.Database.Models;

using System;
using System.ComponentModel.DataAnnotations;

public class MetadataConnector<T>
{
    [Key]
    public Guid Id { get; set; }

    public float? NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public int? TypeValue { get; set; }
    public string Unit { get; set; } = string.Empty;

    public float? MinVal { get; set; }
    public float? MaxVal { get; set; }

    public Guid MetadataId { get; set; }
    public Guid DeviceId { get; set; }

    public virtual Metadata Metadata { get; set; } = null!;
    public virtual T Device { get; set; } = default!;
}
