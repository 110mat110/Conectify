namespace Conectify.Database.Models;

using System;
using System.ComponentModel.DataAnnotations;
using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;

public class Metadata : Serializable, IEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool Exclusive { get; set; } = false;
}
