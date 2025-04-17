using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Automatization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RuleController(RuleService ruleService) : ControllerBase
{
    [HttpGet("create/{behaviourId}")]
    public async Task<IActionResult> AddNewRule(Guid behaviourId)
    {
        var result = await ruleService.AddNewRule(behaviourId, default);

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
    public async Task<IActionResult> SetConnection(Guid idSource, Guid idDestination)
    {
        return await ruleService.SetConnection(idSource, idDestination) ? Ok() : BadRequest();
    }

    [HttpPost("input")]
    public async Task<IActionResult> AddCustomInput(AddActuatorApiModel actuator)
    {
        return await ruleService.AddCustomInput(actuator) ? Ok() : BadRequest();
    }

    [HttpPost("addinputnode")]
    public async Task<IActionResult> AddCustomInputNode(AddInputApiModel inputApiModel)
    {
        return Ok(await ruleService.AddInput(inputApiModel));
    }

    [HttpPost("addoutputnode")]
    public async Task<IActionResult> AddCustomOutputNode(AddOutputApiModel outputApiModel)
    {
        return Ok(await ruleService.AddOutput(outputApiModel));
    }

    [HttpDelete("{ruleId}")]
    public async Task<IActionResult> RemoveCustomInputNode(Guid ruleId)
    {
        return Ok(await ruleService.Remove(ruleId, default));
    }

    [HttpGet("{ruleId}")]
    public async Task<IActionResult> GetRule(Guid ruleId)
    {
        return Ok(await ruleService.GetRule(ruleId));
    }
}
