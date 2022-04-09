namespace Conectify.Database.Models.Values;

using Conectify.Shared.Library.Models.Values;
using System;

public class ApiCommand : ApiBaseModel
{
    public Guid DestinationId { get; set; }
}
