namespace Conectify.Controllers;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    [HttpGet("Ping")]
    public string GetPing()
    {
        return "Hello world";
    }
}
