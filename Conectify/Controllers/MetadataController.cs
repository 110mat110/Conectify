using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MetadataController(IMetadataService metadataService) : ControllerBase
{
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

    [HttpGet("idByCode/{code}")]
    public async Task<IActionResult> MetadataByCode(string code)
    {
        var metadata = await metadataService.GetMetadataByCode(code);
        return metadata is null ? NotFound() : Ok(metadata.Id);
    }

    [HttpDelete("{metadataId}/{deviceId}")]
    public async Task<IActionResult> RemoveMetadata(Guid metadataId, Guid deviceId)
    {
        await metadataService.Remove(metadataId, deviceId);
        return Ok();
    }

    [HttpDelete("{Id}")]
    public async Task<IActionResult> Remove(Guid Id)
    {
        await metadataService.Remove(Id);
        return Ok();
    }
}
