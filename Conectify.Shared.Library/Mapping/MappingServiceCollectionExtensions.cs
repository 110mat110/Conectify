using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Shared.Library.Mapping;

public static class MappingServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IMapper"/> with every <see cref="IMappingProfile"/> found in the given assemblies. Can be called repeatedly, the assemblies add up.</summary>
    public static IServiceCollection AddMapping(this IServiceCollection services, params Assembly[] assemblies)
    {
        var existing = services.FirstOrDefault(x => x.ServiceType == typeof(MappingAssemblies))?.ImplementationInstance as MappingAssemblies;
        if (existing is not null)
        {
            existing.Assemblies.UnionWith(assemblies);
            return services;
        }

        var registered = new MappingAssemblies();
        registered.Assemblies.UnionWith(assemblies);
        services.AddSingleton(registered);
        services.AddSingleton<IMapper>(provider =>
        {
            var configuration = new MapperConfiguration();
            configuration.AddProfiles(provider.GetRequiredService<MappingAssemblies>().Assemblies.ToArray());
            return configuration.CreateMapper();
        });

        return services;
    }

    private sealed class MappingAssemblies
    {
        public HashSet<Assembly> Assemblies { get; } = [];
    }
}
