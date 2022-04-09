namespace Conectify.Shared.Library.Models;

using Conectify.Shared.Library.Interfaces;
using System;

public record ApiDevice : IApiModel
{
    public Guid Id { get; set; }

    public string IPAdress { get; set; } = string.Empty;
    public string MacAdress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? PositionId { get; set; }
}
