using Conectify.Database.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Conectify.Database.Models.Values;
public class Event : IEntity
{
    [Key]
    [JsonIgnore]
    [field: NonSerialized]
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public Guid? DestinationId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string StringValue { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public long TimeCreated { get; set; }
}
