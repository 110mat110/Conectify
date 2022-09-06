namespace Conectify.Database.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public interface IBaseInputType : IEntity
{
    Guid SourceId { get; set; }
    string Name { get; set; }
    string Unit { get; set; }
    string StringValue { get; set; }
    float? NumericValue { get; set; }
    long TimeCreated { get; set; }
}

public record BaseInputType : IBaseInputType
{
    [Key]
    [JsonIgnore]
    [field: NonSerialized]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string StringValue { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public long TimeCreated { get; set; }

    public Guid SourceId { get; set; }
}
