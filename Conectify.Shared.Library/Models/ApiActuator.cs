namespace Conectify.Shared.Library.Models;

using Conectify.Shared.Library.Interfaces;
using System;

public record ApiActuator : IApiModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SourceDeviceId { get; set; }
    public Guid SensorId { get; set; }
}
