namespace Conectify.Database.Interfaces;

using System.Collections.Generic;
using Conectify.Database.Models;

public interface IMetadatable<T> : IEntity
{
    ICollection<MetadataConnector<T>> Metadata { get; set; }
}
