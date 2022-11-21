using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Services;

public static class BehaviourFactory
{
    public static IRuleBehaviour? GetRuleBehaviorByTypeId(Guid id)
    {
        var behaviours = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
           .Where(t => t.GetInterfaces().Contains(typeof(IRuleBehaviour))
        ).Select(x => Activator.CreateInstance(x) as IRuleBehaviour).ToList();

        return behaviours.FirstOrDefault(x => (x)?.GetId() == id);
    }
}
