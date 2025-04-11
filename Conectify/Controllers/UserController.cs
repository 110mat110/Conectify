using Conectify.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController(UserService userService) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<Guid> GetUser(string username)
    {
        return await userService.GetUser(username);
    }
}
