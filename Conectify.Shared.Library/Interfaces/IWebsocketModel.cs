﻿using System;
using Conectify.Shared.Library.Classes;

namespace Conectify.Shared.Library.Interfaces;

public interface IWebsocketModel : ISerializable
{
    Guid Id { get; set; }
}
