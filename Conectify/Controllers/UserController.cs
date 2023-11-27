using Conectify.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Dashboard.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService userService;

    public UserController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet("{username}")]
    public async Task<Guid> GetUser(string username)
    {
        return await userService.GetUser(username);
    }
}
