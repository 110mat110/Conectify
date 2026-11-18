namespace Conectify.Shared.Library.Mapping;

/// <summary>Groups related maps. Implementations need a public parameterless constructor so they can be discovered from an assembly.</summary>
public interface IMappingProfile
{
    void Configure(MapperConfiguration configuration);
}
