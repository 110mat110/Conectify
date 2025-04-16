using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Services;

public static class BehaviorFactory
{
    public static IRuleBehavior GetRuleBehaviorByTypeId(Guid id, IServiceProvider serviceProvider)
    {
        var behaviours = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
           .Where(t => t.GetInterfaces().Contains(typeof(IRuleBehavior))
        ).Select(x => Activator.CreateInstance(x, serviceProvider) as IRuleBehavior).ToList();

        return behaviours.FirstOrDefault(x => (x)?.GetId() == id) ?? throw new Exception($"Cannot load behaviour {id}");
    }
}
