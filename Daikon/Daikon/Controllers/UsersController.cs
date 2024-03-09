using System.Text.Json;
using Daikon.Contracts.Games;
using Daikon.Contracts.Users;
using Daikon.Models;
using Daikon.Services.Users;
using Microsoft.AspNetCore.Mvc;
using PlayFab;

namespace Daikon.Controllers;

[ApiController]
public class UsersController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IAuthService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("Auth")]
    public async Task<IActionResult> AuthUser(AuthUserRequest request)
    {
        var auth = await AuthUserWithSteam(request.SteamTicket);
        _logger.LogInformation("Auth was requested:" + request.SteamTicket);
        if(auth)
        {
            var token = Guid.NewGuid();
            _userService.AddNewToken(token, DateTime.Now);
            var response = new AuthUserResponse(){
                SessionToken = token,
            };
            return Ok(response);
        }
        else return NoContent();
    }
    
    [HttpGet("Clean")]
    public async Task<IActionResult> CleanRepo(AdminCleanup cleanup)
    {
        if(cleanup.Key != "smick")
        {
            return NoContent();
        }
        PlayFabSettings.staticSettings.TitleId= "5FDDE";
        PlayFabSettings.staticSettings.DeveloperSecretKey ="SJBDAIQS9GIDTBCKM3K5FXXFYAHM9Z6QEB8R1Z9BFSUOEN39OR";
        var pfToken = await PlayFabAuthenticationAPI.GetEntityTokenAsync(new PlayFab.AuthenticationModels.GetEntityTokenRequest());

        if(pfToken.Error != null)
        {
            _logger.Log(LogLevel.Information, "Could Not auth title..");
            return NoContent();
        }
        else 
        {
            var tryDelete = await PlayFabMultiplayerAPI.DeleteContainerImageRepositoryAsync(new PlayFab.MultiplayerModels.DeleteContainerImageRequest()
            {
                ImageName = cleanup.Repo
            });
            if(tryDelete.Error != null)
            {
                _logger.Log(LogLevel.Information, "Could not delete mdmc.." + tryDelete.Error.ErrorMessage);
            }
            else
            {
                _logger.Log(LogLevel.Information, "repository deleted? check playfab!");
            }
        }
        return NoContent();
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