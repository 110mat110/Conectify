using System;

namespace Conectify.Shared.Library.Models;

public class ApiBasicMetadata
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool Exclusive { get; set; } = false;
}
