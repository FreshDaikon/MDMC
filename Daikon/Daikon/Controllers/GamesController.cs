using Daikon.Contracts.Games;
using Daikon.Models;
using Daikon.Services.Games;
using Daikon.Services.Users;
using Microsoft.AspNetCore.Mvc;
using PlayFab;

namespace Daikon.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{

    private readonly IAuthService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly IGameManager _gameManager;

    public GamesController(IAuthService userService, IGameManager gameManager, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
        _gameManager = gameManager;
    }

    [HttpPost("/server/hearbeat")]
    public IActionResult HeartBeat(string sessionId)
    {
        _gameManager.NewHeartBeat(sessionId);
        return Ok("ok");
    }

    [HttpGet("create")]
    public async Task<IActionResult> NewGame(NewGameRequest request)
    {
        var auth = _userService.ValidateToken(request.SessionToken);
        if(auth)
        {
            _logger.LogInformation("Success in authing user - request new server");
            PlayFabSettings.staticSettings.TitleId= "";
            PlayFabSettings.staticSettings.DeveloperSecretKey ="";
            var pfToken = await PlayFabAuthenticationAPI.GetEntityTokenAsync(new PlayFab.AuthenticationModels.GetEntityTokenRequest());
            // Could not auth this Controller.
            if(pfToken.Error != null)
            {
                _logger.Log(LogLevel.Information, ("Could Not auth title.." + pfToken.Error.ErrorMessage));
                return NoContent();
            }
            else 
            {
                var buildIds = await PlayFabMultiplayerAPI.ListBuildSummariesV2Async(new PlayFab.MultiplayerModels.ListBuildSummariesRequest());
                //Could Not Retrieve build list..
                if(buildIds.Error != null)
                {
                    _logger.Log(LogLevel.Information, ("Could Not Create Server.." + buildIds.Error.ErrorMessage));
                    return NoContent();
                }
                // OK got a list of builds..
                else
                {
                    if(buildIds.Result.BuildSummaries.Count > 0)
                    {
                        var latest = buildIds.Result.BuildSummaries[0].BuildId;
                        var _sessionId = Guid.NewGuid().ToString();
                        _logger.Log(LogLevel.Information, "The requested arena was : " + request.Arena);
                        var Server = await PlayFabMultiplayerAPI.RequestMultiplayerServerAsync(new PlayFab.MultiplayerModels.RequestMultiplayerServerRequest()
                        {
                            BuildId = latest,
                            PreferredRegions = new[] {"NorthEurope"}.ToList(),
                            SessionId = _sessionId,
                            SessionCookie = request.Arena
                        });
                        // We Could Not get a Server:
                        if(Server.Error != null)
                        {
                            _logger.Log(LogLevel.Information, "Could Not Create Server..");
                            _logger.Log(LogLevel.Information, Server.Error.ErrorMessage);
                            return NoContent();          
                        }
                        else
                        {
                            var result = Server.Result;
                            var joinCode = GenerateJoinCode(10);
                            //
                            var NewGame = new Game
                            {
                                SessionId = _sessionId.ToString(),
                                JoinCode = joinCode,
                                ServerHost = result.FQDN,
                                ServerPort = result.Ports.First(port => port.Name == "GameServer").Num,
                                LastHeartBeat = DateTime.Now
                            };
                            _gameManager.AddGame(NewGame);
                            var response = new GameCreatedResponse(){
                                JoinCode = joinCode, 
                                ServerHost = result.FQDN,
                                ServerPort = result.Ports.First(port => port.Name == "GameServer").Num
                            };
                            return Ok(response);
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, "Could Not get a server because the list is empty!");
                    }
                }
            }
        }
        return NoContent();
    }
    
    [HttpGet("join")]
    public IActionResult JoinGame(JoinGameRequest request)
    {
        _logger.Log(LogLevel.Information, "Let's try and auth first.");
        var auth = _userService.ValidateToken(request.SessionToken);
        if(auth)
        {
            var game = _gameManager.GetGame(request.JoinCode);
            if(game != null)
            {
                _logger.Log(LogLevel.Information, "Game Found!");
                var response = new GameFoundReponse()
                {
                    ServerHost = game.ServerHost,
                    ServerPort = game.ServerPort
                };
                return Ok(response);
            }
            _logger.Log(LogLevel.Information, "We could not find the requested game."); 
            return NoContent();
        }
        return NoContent();
    }
    
    private string GenerateJoinCode(int length)
    {
        var _random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
