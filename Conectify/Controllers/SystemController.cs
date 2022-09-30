namespace Conectify.Server.Controllers;

using Conectify.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    private readonly ConectifyDb database;

    public SystemController(ConectifyDb database)
    {
        this.database = database;
    }

    [HttpGet("Ping")]
    public string GetPing()
    {
        return "Hello world";
    }

    [HttpGet("Time")]
    public long GetTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    [HttpGet("UpdateDatabase")]
    public string UpdateDatabase()
    {
        database.Database.Migrate();
        return "database update";
    }
}
