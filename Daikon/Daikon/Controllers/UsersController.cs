using System.Text.Json;
using Daikon.Contracts.Games;
using Daikon.Contracts.Users;
using Daikon.Models;
using Daikon.Models.SteamUtils;
using Daikon.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace Daikon.Controllers;

[ApiController]
[Route("auth")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAuthService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet()]
    public async Task<IActionResult> AuthUser(AuthUserRequest request)
    {
        var auth = await AuthUserWithSteam(request.SteamTicket);
        if(auth)
        {
            var token = Guid.NewGuid();
            _userService.AddNewToken(token, DateTime.Now);
            var response = new AuthUserResponse(token);
            return Ok(response);
        }
        else return NoContent();
    }
    
    private async Task<bool> AuthUserWithSteam(string ticket)
    {
        await Task.Delay(100);
        if(ticket == "bannana")
        {
            return true;
        }
        return false;
    }

}