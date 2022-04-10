﻿namespace Conectify.Server.Controllers;

using Conectify.Database.Interfaces;
using Conectify.Server.Services;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController, Route("api/[controller]")]
public class DeviceControllerBase<TApi> : ControllerBase where TApi : IApiModel
{
    private readonly IUniversalDeviceService<TApi> service;
    private readonly ILogger<DeviceControllerBase<TApi>> logger;

    public DeviceControllerBase(ILogger<DeviceControllerBase<TApi>> logger, IUniversalDeviceService<TApi> service)
    {
        this.logger = logger;
        this.service = service;
    }

    [HttpPost()]
    public async Task<IActionResult> AddNew(TApi apiDevice, CancellationToken ct)
    {
        try
        {
            return (await service.AddKnownDevice(apiDevice, ct)) ? Ok() : BadRequest();

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await service.GetAllDevices(ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecific(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await service.GetSpecificDevice(id, ct);
            return result != null ? new ObjectResult(result) : NotFound();

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot download device");
        }
    }

    [HttpPost("metadata")]
    public async Task<IActionResult> AddMetadata(ApiMetadataConnector metadata, CancellationToken ct = default)
    {
       if(await service.AddMetadata(metadata, ct))
        {
            return Ok();
        }
       return BadRequest();
    }

    [HttpGet("{id}/metadata")]
    public async Task<IActionResult> GetMetadata(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await service.GetSpecificDevice(id, ct);
            return result != null ? new ObjectResult(result) : NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot download device");
}
    }
}