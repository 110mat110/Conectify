namespace Conectify.Database.Interfaces;

using Conectify.Database.Models;
using System.Collections.Generic;

public interface IMetadatable : IEntity
{
    List<Metadata> Metadata { get; set; }
}
