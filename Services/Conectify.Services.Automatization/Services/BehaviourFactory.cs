using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Services;

public static class BehaviourFactory
{
    public static IRuleBehaviour? GetRuleBehaviorByTypeId(Guid id)
    {
        var behaviours = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x =>
            !x.IsAbstract
            && !x.IsInterface
            && x.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IRuleBehaviour)
            )
        ).Select(x => Activator.CreateInstance(x) as IRuleBehaviour).ToList();

        return behaviours.FirstOrDefault(x => (x)?.GetId() == id);
    }
}
