using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Conectify.Shared.Library.Mapping;

internal sealed class Mapper(MapperConfiguration configuration) : IMapper
{
    private readonly ConcurrentDictionary<(Type Source, Type Destination), Func<object, object?>?> compiled = new();

    public TDestination Map<TDestination>(object? source)
    {
        if (source is null)
        {
            return default!;
        }

        return (TDestination)MapToType(source, typeof(TDestination))!;
    }

    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        var definition = configuration.Find(source.ElementType, typeof(TDestination))
            ?? throw new InvalidOperationException($"No map from {source.ElementType.Name} to {typeof(TDestination).Name}");

        var select = Expression.Call(
            typeof(Queryable),
            nameof(Queryable.Select),
            [source.ElementType, typeof(TDestination)],
            source.Expression,
            Expression.Quote(definition.Expression));

        return source.Provider.CreateQuery<TDestination>(select);
    }

    private object? MapToType(object source, Type destination)
    {
        var sourceType = source.GetType();
        var map = GetCompiledMap(sourceType, destination);
        if (map is not null)
        {
            return map(source);
        }

        if (source is IEnumerable items && source is not string && TryGetElementType(destination, out var destinationElement))
        {
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(destinationElement))!;
            foreach (var item in items)
            {
                list.Add(item is null ? null : MapToType(item, destinationElement));
            }

            return list;
        }

        if (destination.IsAssignableFrom(sourceType))
        {
            return source;
        }

        throw new InvalidOperationException($"No map from {sourceType.Name} to {destination.Name}");
    }

    private Func<object, object?>? GetCompiledMap(Type source, Type destination)
    {
        return compiled.GetOrAdd((source, destination), key =>
        {
            var definition = configuration.Find(key.Source, key.Destination);
            if (definition is null)
            {
                return null;
            }

            var input = Expression.Parameter(typeof(object), "source");
            var body = Expression.Convert(Expression.Invoke(definition.Expression, Expression.Convert(input, definition.Expression.Parameters[0].Type)), typeof(object));
            return Expression.Lambda<Func<object, object?>>(body, input).Compile();
        });
    }

    /// <summary>True when a List of the returned element type can be assigned to <paramref name="collection"/> (List, IList, ICollection, IEnumerable, ...).</summary>
    private static bool TryGetElementType(Type collection, out Type element)
    {
        element = typeof(object);
        if (!collection.IsGenericType || collection.GetGenericArguments().Length != 1)
        {
            return false;
        }

        element = collection.GetGenericArguments()[0];
        return collection.IsAssignableFrom(typeof(List<>).MakeGenericType(element));
    }
}
