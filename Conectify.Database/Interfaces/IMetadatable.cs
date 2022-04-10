namespace Conectify.Database.Interfaces;

using Conectify.Database.Models;
using System.Collections.Generic;

public interface IMetadatable<T> : IEntity
{
    IEnumerable<MetadataConnector<T>> Metadata { get; set; }
}
