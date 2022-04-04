namespace Conectify.Shared.Library.Models;

using Conectify.Shared.Library.Classes;
using Conectify.Shared.Library.Interfaces;
using System;

public class ApiValueModel : Serializable, IApiModel
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Guid? DestinationId { get; set; }
    public Guid SourceId { get; set; }
    public long TimeCreated { get; set; }
    public string Name { get; set; }
    public string StringValue { get; set; }
    public float? NumericValue { get; set; }
    public string Unit { get; set; }
}
