using System.Reflection;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Services;

public static class BehaviorFactory
{
    public static IEnumerable<IRuleBehavior> GetAllBehaviours(IServiceProvider serviceProvider)
    {
        return GetBehaviourTypes()
            .Select(t => Activator.CreateInstance(t, serviceProvider) as IRuleBehavior)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    public static IRuleBehavior GetRuleBehaviorByTypeId(Guid id, IServiceProvider serviceProvider)
    {
        return GetAllBehaviours(serviceProvider).FirstOrDefault(x => x.GetId() == id)
            ?? throw new Exception($"Cannot load behaviour {id}");
    }

    // Only scan the assembly that defines the rule behaviours. Scanning the whole
    // AppDomain used to throw ReflectionTypeLoadException intermittently under parallel
    // tests, because FakeItEasy/AutoMapper emit dynamic proxy assemblies whose types
    // cannot always be enumerated while they are being generated on another thread.
    private static IEnumerable<Type> GetBehaviourTypes()
    {
        return GetLoadableTypes(typeof(IRuleBehavior).Assembly)
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IRuleBehavior).IsAssignableFrom(t));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).Select(t => t!);
        }
    }
}
