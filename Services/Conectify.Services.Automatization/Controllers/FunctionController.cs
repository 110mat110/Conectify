using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Automatization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FunctionController(FunctionService functionService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IEnumerable<GetFunctionApiModel>> GetAllFunctions()
    {
        return await functionService.GetAllFunctions();
    }

    [HttpPost]
    public async Task<IActionResult> AddFunction(CreateFunctionApiModel function)
    {
        return Ok(await functionService.AddFunction(function));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditFunction(Guid id, EditFunctionApiModel function)
    {
        return await functionService.EditFunction(id, function) ? Ok() : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFunction(Guid id)
    {
        return await functionService.RemoveFunction(id) ? Ok() : BadRequest();
    }

    [HttpPost("{id}/port")]
    public async Task<IActionResult> AddPort(Guid id, AddFunctionPortApiModel port)
    {
        return Ok(await functionService.AddPort(id, port));
    }

    /// <summary>Moves a rule into a function, an empty function id moves it onto the top level board.</summary>
    [HttpPut("rule/{ruleId}")]
    public async Task<IActionResult> MoveRule(Guid ruleId, MoveToFunctionApiModel target)
    {
        return await functionService.MoveRule(ruleId, target.FunctionId) ? Ok() : BadRequest();
    }

    [HttpPut("{id}/parent")]
    public async Task<IActionResult> MoveFunction(Guid id, MoveToFunctionApiModel target)
    {
        return await functionService.MoveFunction(id, target.FunctionId) ? Ok() : BadRequest();
    }
}
