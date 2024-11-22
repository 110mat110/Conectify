using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Rules;
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

        return types.Where(x => x is not null).Select(x => new BehaviourMenuApiModel(x!.GetId(), x.DisplayName(), x.DefaultOutputs, x.DefaultInputs));
    }
}
