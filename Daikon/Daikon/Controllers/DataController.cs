using Daikon.Contracts.Data;
using Daikon.Contracts.Models;
using Daikon.InternalContracts;
using Daikon.Services.Users;
using Microsoft.AspNetCore.Mvc;
using FireSharp; 
using PlayFab;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp.Response;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Daikon.Controllers;

[ApiController]
[Route("data")]
public class DataController : ControllerBase
{
    private readonly IFirebaseClient _client;
    private readonly ILogger<DataController> _logger;

    private IAuthService _userService;
    
    public DataController(ILogger<DataController> logger, IAuthService userService)
    {
        _logger = logger;
        _userService = userService;
        IFirebaseConfig config = new FirebaseConfig {
            AuthSecret = "K5bOXRWuH2JLTWoNsYWZeT54CwAEwPteVqySRV10",
            BasePath = "https://mdmc-fe55d-default-rtdb.europe-west1.firebasedatabase.app"
        };
        _client = new FirebaseClient(config);
    }
 
    [HttpPost("SetArenaRecord")]
    public async Task<IActionResult> TrySetArenaData(SetArenRecordRequest request)
    {  
        if(request.ServerKey != "bananna")
        {
            _logger.Log(LogLevel.Information, "could not auth game server - don't update..");
            //Server failed to auth..
            return NoContent();
        }
        _logger.Log(LogLevel.Information, "Try and update Firebase..");
        var test = await _client.SetAsync("ArenaRecords/" + request.Id + "/" +
                                          (request.Progress == 1 ? "Clears" : "Progs")
                                          + "/" + request.Session, new UpdateArena
        {
            Runtime = request.Runtime,
            DateTime = DateTime.Now.ToUniversalTime().Ticks,
            Participants = request.Participants,
            Progress = request.Progress,
        });

        return Ok();
    }
    
    [HttpGet("GetArenaRecords")]
    public async Task<IActionResult> GetArenaRecords(GetArenaRecordsRequest request)
    {
        if (!_userService.ValidateToken(request.SessionToken)) return NoContent();
        
        _logger.Log(LogLevel.Information, "Try and get arena data..");
        var response = await _client.GetAsync("ArenaRecords/", FireSharp.QueryBuilder.New().OrderBy("DateTime").LimitToFirst(10));
        dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

        var list = new List<ArenaRecord>();

        foreach (var itemDynamic in data)
        {
            list.Add(JsonConvert.DeserializeObject<ArenaRecord>(((JProperty)itemDynamic).Value.ToString()));
        }
        
        var records = new GetArenaRecordsResponse()
        {
            Records = list.ToArray()
        };
        return Ok(records);
    }
}

