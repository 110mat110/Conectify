using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Automatization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RuleController : ControllerBase
{
    private readonly RuleService ruleService;

    public RuleController(RuleService ruleService)
    {
        this.ruleService = ruleService;
    }

    [HttpPost]
    public async Task<IActionResult> AddNewRule(CreateRuleApiModel ruleApiModel)
    {
        var result = await ruleService.AddNewRule(ruleApiModel, default);

        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRules()
    {
        return Ok(await ruleService.GetAllRules());
    }

    [HttpGet("connections")]
    public async Task<IEnumerable<ConnectionApiModel>> GetAllConnections()
    {
        return await ruleService.GetAllConnections();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditRule(Guid id, EditRuleApiModel rule)
    {
        return Ok(await ruleService.EditRule(id, rule));
    }

    [HttpPost("connection/{idSource}/{idDestination}")]
    public async Task<IActionResult> AddConnection(Guid idSource, Guid idDestination)
    {
        return await ruleService.AddConnection(idSource, idDestination) ? Ok() : BadRequest();
    }

    [HttpDelete("connection/{idSource}/{idDestination}")]
    public async Task<IActionResult> RemoveConnection(Guid idSource, Guid idDestination)
    {
        return await ruleService.RemoveConnection(idSource, idDestination) ? Ok() : BadRequest();
    }

    [HttpPost("input")]
    public async Task<IActionResult> AddCustomInput(AddActuatorApiModel actuator)
    {
        return await ruleService.AddCustomInput(actuator) ? Ok() : BadRequest();
    }
}
