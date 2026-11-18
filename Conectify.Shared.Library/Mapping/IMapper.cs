using System;
using System.Collections.Generic;
using System.Linq;

namespace Conectify.Shared.Library.Mapping;

public interface IMapper
{
    /// <summary>Maps <paramref name="source"/> to <typeparamref name="TDestination"/>; a null source gives default. Collections are mapped element by element.</summary>
    TDestination Map<TDestination>(object? source);

    /// <summary>Appends the registered projection to the query so it is translated by the query provider (SQL for EF).</summary>
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
}

public static class QueryableMappingExtensions
{
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IMapper mapper) => mapper.ProjectTo<TDestination>(source);
}
