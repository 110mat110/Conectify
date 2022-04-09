namespace Conectify.Database.Models.Values;

using Conectify.Shared.Library.Models.Values;
using System;

public class ApiCommandResponse : ApiBaseModel
{
    public Guid CommandId { get; set; }
}
