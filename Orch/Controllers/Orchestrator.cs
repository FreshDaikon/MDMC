using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text.Json.Nodes;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.Data;
using System.Linq;

[ApiController]
[Route("[controller]")]
public class Orchestrator : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Orchestrator> _logger;
    private readonly Random _random;

    private const string KeyParamName = "PlayFabTitle";
    private const string TitleParamName ="PlayFabKey";
    private const string TempTokenParmName = "TempToken";
    private const string SteamKeyParamName = "";

    public Orchestrator(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<Orchestrator> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _random = new Random();
    }

    [HttpGet("/Orch")]
    public async Task Get() 
    {
        _logger.Log(LogLevel.Information, "Welcome to Orch1.0");
        if (HttpContext.WebSockets.IsWebSocketRequest) 
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            // TODO add steam back:
            var client = AuthenticateRequest(HttpContext.Request);

            if (client == null) 
            {
                var closeMessage = new 
                {
                    MessageType = OrchMessageType.ConnectionError,
                    MessageContent = new ConnectionErrorMessage()
                    {
                        Reason = "Could not Authenticate Player."
                    }
                };
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, JsonSerializer.Serialize(closeMessage), CancellationToken.None);
            } 
            else 
            {
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                OrchConnections.Connections.TryAdd(webSocket, client);
                _logger.Log(LogLevel.Information, "Number of connections :" + OrchConnections.Connections.Count);
                await HandleWebSocketConnection(webSocket, client);
            }
        } 
        else 
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [HttpGet("/Banana")]
    public string? Banana()
    {
        _logger.Log(LogLevel.Information, "Welcome to Orch1.0 - fetching banana...");
        return GenerateJoinCode(20);
    }

    private async Task HandleWebSocketConnection(WebSocket webSocket, OrchClient client)
    {
        await SendMessageToConnection(webSocket, new {
            MessageType = OrchMessageType.ConnectionEstablished,
            MessageContent = new ConnectionEstablishedMessage() {
                Username = client.Username,
                WelcomeMessage = "Hey, welcome mate!"
            }
        });

        _logger.Log(LogLevel.Information, "Welcome message sent");
        _logger.Log(LogLevel.Information, "Number of connections :" + OrchConnections.Connections.Count);
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue) 
        {
            await HandleMessage(webSocket, client, buffer);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        _logger.Log(LogLevel.Information, "WebSocket connection closed");
        OrchConnections.Connections.TryRemove(webSocket, out client!);
    }

    private OrchClient? AuthenticateRequest(HttpRequest request) 
    {
        StringValues verificationTicket;
        string ClientKey = _configuration[TempTokenParmName];
        string ServerKey = _configuration[TempTokenParmName];

        bool TicketResult = request.Headers.TryGetValue("keyToken", out verificationTicket);
        _logger.Log(LogLevel.Information, request.Headers.ToString());
        
        if(TicketResult)
        {
            if(verificationTicket.ToString() == ClientKey)
            {
                return new OrchClient()
                {
                    ServerKey = ""
                };
            }
            else if(verificationTicket.ToString() == ServerKey)
            {
                return new OrchClient()
                {
                    ServerKey = ""
                };
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    private async Task SendMessageToConnection(WebSocket webSocket, dynamic message) 
    {
        var messageContent = JsonSerializer.Serialize(message);
        var serverMsg = Encoding.UTF8.GetBytes(messageContent);
        await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task HandleMessage(WebSocket webSocket, OrchClient client, byte[] buffer) 
    {
        var message = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        var messageData = JsonSerializer.Deserialize<JsonObject>(message);

        switch ((ClientMessageType) (int) messageData!["MessageType"]!) 
        {
            case ClientMessageType.RequestGame:
                var requestContent = messageData["MessageContent"].Deserialize<RequestGameMessage>();
                var arena = requestContent?.Arena;
                await SendMessageToConnection(webSocket, new {
                    MessageType = OrchMessageType.RequestingGame,
                });
                if(arena != null)
                {
                    await TryCreateGame(webSocket, client, arena);
                }
                else
                {
                    await TryCreateGame(webSocket, client, "none");
                }
                break;
            case ClientMessageType.JoinGame:
                var joinContent = messageData["MessageContent"].Deserialize<JoinGameMessage>();
                var code = joinContent?.ShareableCode;
                if(code != null)
                {
                    await TryJoinGame(webSocket, code, client);  
                }
                break;
        }
    }
    
    private async Task TryCreateGame(WebSocket sourceSocket, OrchClient client, string arena)
    {
        PlayFabSettings.staticSettings.TitleId = _configuration[TitleParamName]; 
        PlayFabSettings.staticSettings.DeveloperSecretKey = _configuration[KeyParamName];

        var authToken = await PlayFabAuthenticationAPI.GetEntityTokenAsync(new PlayFab.AuthenticationModels.GetEntityTokenRequest());
        if(authToken.Error != null)
        {
            _logger.Log(LogLevel.Information, "Could not auth title..");
            await SendMessageToConnection(sourceSocket, new {
                MessageType = OrchMessageType.GameRequestFailed,
                MessageContent = new GameRequstFailedMessage(){
                    Reason = "Request could not be transactioned."
                }
            });
        }
        else
        {
            var buildIds = await PlayFabMultiplayerAPI.ListBuildSummariesV2Async(new PlayFab.MultiplayerModels.ListBuildSummariesRequest());
            if(buildIds.Error != null)
            {
                _logger.Log(LogLevel.Information, "Could not get build ids..");
                await SendMessageToConnection(sourceSocket, new {
                    MessageType = OrchMessageType.GameRequestFailed,
                    MessageContent = new GameRequstFailedMessage(){
                        Reason = "No valid builds could be found."
                    }
                });
            }
            else
            {
                if(buildIds.Result.BuildSummaries.Count > 0)
                {
                    var latest = buildIds.Result.BuildSummaries[0].BuildId;
                    var SessionId = Guid.NewGuid().ToString();
                    var Server = await PlayFabMultiplayerAPI.RequestMultiplayerServerAsync(new PlayFab.MultiplayerModels.RequestMultiplayerServerRequest()
                    {
                        BuildId = latest,
                        PreferredRegions = new[] {"NorthEurope"}.ToList(),
                        SessionId = SessionId,
                        SessionCookie = arena
                    });
                    if(Server.Error != null)
                    {
                        _logger.Log(LogLevel.Information, "Could Not Create Server..");
                        _logger.Log(LogLevel.Information, Server.Error.ErrorMessage);
                        await SendMessageToConnection(sourceSocket, new {
                            MessageType = OrchMessageType.GameRequestFailed,
                            MessageContent = new GameRequstFailedMessage(){
                                Reason = "Could not request any more Servers (Do queue stuff here..)"
                            }
                        });
                    }
                    else
                    {
                        var apiResult = Server.Result;
                        var joinCode = GenerateJoinCode(10);
                        client.ServerKey = joinCode;
                        OrchGame newGame = new OrchGame()
                        { 
                            SessionId = SessionId,
                            Host = apiResult.FQDN,
                            Port = apiResult.Ports.First(port => port.Name == "GameServer")?.Num,
                            JoinCode = joinCode,

                        };
                        OrchGameList.Games.TryAdd(joinCode, newGame);

                        var ServerCreatedMessage = new {
                            MessageType = OrchMessageType.GameCreated,
                            MessageContent = new GameCreatedMessage()
                            {
                                ServerUrl = newGame.Host,
                                ServerPort = newGame.Port,
                                ShareableCode = newGame.JoinCode
                            }
                        };
                        await SendMessageToConnection(sourceSocket, ServerCreatedMessage);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Information, "Could Not Create Server..");
                    await SendMessageToConnection(sourceSocket, new {
                        MessageType = OrchMessageType.GameRequestFailed,
                        MessageContent = new GameRequstFailedMessage(){
                            Reason = "Zero builds found."
                        }
                    });
                }
                
            }
        }
    }

    private async Task TryJoinGame(WebSocket sourceSocket, string joinCode, OrchClient client)
    {
        OrchGame game;
        if(OrchGameList.Games.TryGetValue(joinCode, out game))
        {
            if(game != null)
            {                
                client.ServerKey = joinCode;
                var ServerFoundMessage = new {
                    MessageType = OrchMessageType.GameFound,
                    MessageContent = new GameFoundMessage()
                    {
                        ServerUrl = game.Host,
                        ServerPort = (int)game.Port,
                    }
                };
                await SendMessageToConnection(sourceSocket, ServerFoundMessage);
            }
            else
            {
                _logger.Log(LogLevel.Information, "Could Not Find Server..");
                await SendMessageToConnection(sourceSocket, new {
                    MessageType = OrchMessageType.GameRequestFailed,
                    MessageContent = new GameRequstFailedMessage(){
                        Reason = "No Server with that join Code Found"
                    }
                });
            }
        }
        else
        {
            _logger.Log(LogLevel.Information, "Could Not Find Server to join..");
            await SendMessageToConnection(sourceSocket, new {
                MessageType = OrchMessageType.GameRequestFailed,
                MessageContent = new GameRequstFailedMessage(){
                    Reason = "No servers found.."
                }
            });
        }        
    }

    private string GenerateJoinCode(int length)
    {
        var _random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
