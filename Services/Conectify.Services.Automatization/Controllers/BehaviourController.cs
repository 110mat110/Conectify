using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Automatization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BehaviourController(IServiceProvider serviceProvider) : ControllerBase
{
    [HttpGet("all")]
    public IEnumerable<BehaviourMenuApiModel> GetAllBehaviours()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
       .Where(t => t.GetInterfaces().Contains(typeof(IRuleBehaviour))).Select(x => Activator.CreateInstance(x, serviceProvider) as IRuleBehaviour).ToList();

        return types.Where(x => x is not null).Select(x => new BehaviourMenuApiModel(x!.GetId(), x.DisplayName(), x.Outputs, x.Inputs));
    }

    [HttpGet("{id}")]
    public BehaviourMenuApiModel? GetBehaviour(Guid id)
    {
        var x = BehaviourFactory.GetRuleBehaviorByTypeId(id, serviceProvider);

        if (x is null) return null;

        return new BehaviourMenuApiModel(x!.GetId(), x.DisplayName(), x.Outputs, x.Inputs);
    }
}
