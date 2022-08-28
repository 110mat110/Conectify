using Conectify.Database;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Automatization.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetAllRules()
        {
            return Ok(await ruleService.GetAllRules());
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditRule(Guid id, string propertyJson)
        {
            return Ok(await ruleService.EditRule(id, propertyJson));
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
    }
}
