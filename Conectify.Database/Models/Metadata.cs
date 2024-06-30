namespace Conectify.Database.Models;

using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Classes;
using System;
using System.ComponentModel.DataAnnotations;

public class Metadata : Serializable, IEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set;} = string.Empty;
    public bool Exclusive { get; set; } = false;
}
