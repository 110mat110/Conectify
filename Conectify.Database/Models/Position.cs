namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Position : IEntity
{
    [Key]
    [JsonIgnore]
    [field: NonSerialized]
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public float Lat { get; set; }
    public float Long { get; set; }
}
