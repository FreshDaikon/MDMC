using Daikon.Contracts.Games;
using Microsoft.AspNetCore.Mvc;

namespace Daikon.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{

    [HttpPost()]
    public IActionResult NewGame(NewGameRequest request)
    {
        return Ok(request);
    }

    [HttpGet()]
    public IActionResult JoinGame(JoinGameRequest request)
    {
        return Ok(request);
    }

}
