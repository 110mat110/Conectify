using Conectify.Shared.Library.Classes;
using System;

namespace Conectify.Shared.Library.Interfaces;

public interface IWebsocketModel : ISerializable
{
    Guid Id { get; set; }
}
