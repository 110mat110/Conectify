using Conectify.Services.Dashboard.Models;
using Conectify.Services.Dashboard.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Conectify.Services.Dashboard.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{

    // GET: api/<DashboardController>
    [HttpGet("all/{userId}")]
    public async Task<IEnumerable<DashboardApi>> GetAll(Guid userId)
    {
        return await dashboardService.GetDashboards(userId);
    }

    // GET api/<DashboardController>/5
    [HttpGet("{id}")]
    public async Task<DashboardApi> Get(Guid id)
    {
        return await dashboardService.GetDashboard(id);
    }

    // POST api/<DashboardController>
    [HttpPost]
    public async Task<DashboardApi> Post([FromBody] AddDashboardApi dashboard)
    {
        return await dashboardService.Add(dashboard);
    }

    // PUT api/<DashboardController>/5
    [HttpPut("{id}")]
    public async Task Put(Guid id, [FromBody] EditDashboardApi dashboard)
    {
        await dashboardService.Edit(id, dashboard, default);
    }

    // DELETE api/<DashboardController>/5
    [HttpDelete("{id}")]
    public async Task Delete(Guid id)
    {
       await dashboardService.Remove(id);
    }

    // POST api/<DashboardController>
    [HttpPost("{id}/Device")]
    public async Task<Guid> DevicePost(Guid id,[FromBody] AddDeviceApi device)
    {
        return await dashboardService.AddDevice(id, device, default);
    }

    // PUT api/<DashboardController>/5
    [HttpPut("{id}/Device")]
    public async Task Put(Guid id,[FromBody] EditDeviceApi dashboard)
    {
        await dashboardService.EditDevice(dashboard, default);
    }

    // DELETE api/<DashboardController>/5
    [HttpDelete("{id}/Device/{idDevice}")]
    public async Task Delete(Guid id, Guid idDevice)
    {
        await dashboardService.RemoveDevice(idDevice);
    }
}
