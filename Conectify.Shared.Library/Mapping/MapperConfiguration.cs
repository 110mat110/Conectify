using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Conectify.Shared.Library.Mapping;

/// <summary>
/// Registry of explicit maps. Every map is an expression that builds the destination with an object initializer, so the same
/// definition is compiled for in-memory mapping and handed to the query provider for projections.
/// </summary>
public sealed class MapperConfiguration
{
    private readonly Dictionary<(Type Source, Type Destination), MapDefinition> maps = new();

    public MapperConfiguration()
    {
    }

    public MapperConfiguration(Action<MapperConfiguration> configure)
    {
        configure(this);
    }

    public void AddProfile<TProfile>() where TProfile : IMappingProfile, new() => AddProfile(new TProfile());

    public void AddProfile(IMappingProfile profile) => profile.Configure(this);

    public void AddProfiles(params Assembly[] assemblies)
    {
        var profileTypes = assemblies
            .Distinct()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IMappingProfile).IsAssignableFrom(x) && x is { IsAbstract: false, IsInterface: false } && x.GetConstructor(Type.EmptyTypes) is not null);

        foreach (var profileType in profileTypes)
        {
            AddProfile((IMappingProfile)Activator.CreateInstance(profileType)!);
        }
    }

    /// <summary>
    /// Registers a map. <paramref name="map"/> must be an object initializer; every settable destination property has to be
    /// either set by it or listed in <paramref name="ignoredMembers"/> (checked by <see cref="AssertConfigurationIsValid"/>).
    /// </summary>
    public void CreateMap<TSource, TDestination>(Expression<Func<TSource, TDestination>> map, params string[] ignoredMembers)
    {
        maps[(typeof(TSource), typeof(TDestination))] = new MapDefinition(map, ignoredMembers.ToHashSet());
    }

    public IMapper CreateMapper() => new Mapper(this);

    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var ((source, destination), definition) in maps)
        {
            var name = $"{source.Name} -> {destination.Name}";

            if (definition.Expression.Body is not MemberInitExpression init)
            {
                errors.Add($"{name}: the map has to be an object initializer");
                continue;
            }

            var setProperties = destination
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.SetMethod is { IsPublic: true })
                .Select(x => x.Name)
                .ToHashSet();
            var bound = init.Bindings.Select(x => x.Member.Name).ToHashSet();

            errors.AddRange(setProperties.Except(bound).Except(definition.IgnoredMembers).Select(x => $"{name}: {x} is neither mapped nor ignored"));
            errors.AddRange(definition.IgnoredMembers.Except(setProperties).Select(x => $"{name}: ignored member {x} does not exist"));
            errors.AddRange(definition.IgnoredMembers.Intersect(bound).Select(x => $"{name}: {x} is both mapped and ignored"));
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("Invalid mapper configuration:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        }
    }

    internal MapDefinition? Find(Type source, Type destination)
    {
        for (var type = source; type is not null; type = type.BaseType)
        {
            if (maps.TryGetValue((type, destination), out var definition))
            {
                return definition;
            }
        }

        return null;
    }
}

internal sealed record MapDefinition(LambdaExpression Expression, IReadOnlySet<string> IgnoredMembers);
