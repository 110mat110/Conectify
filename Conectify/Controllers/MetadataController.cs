using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        private readonly IMetadataService metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            this.metadataService = metadataService;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<ApiBasicMetadata>> GetAllMetadataAsync()
        {
            return await metadataService.GetAllMetadata();
        }

        [HttpPost()]
        public async Task<IActionResult> AddNewMetadataAsync(ApiBasicMetadata metadata)
        {
            return await metadataService.AddNewMetadata(metadata) ? Ok() : BadRequest();
        }
    }
}
