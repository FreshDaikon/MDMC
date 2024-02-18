using System.Text;
using Daikon.Contracts.Users;
using Daikon.Helpers;
using Godot;
using Newtonsoft.Json;

namespace Daikon.Client.Connect;

public partial class DaikonConnect: Node
{
    public static DaikonConnect Instance;   

    
    // Various Configs:
    public bool ConnectOnline = false;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        base._Ready();
    }

    public void AuthDaikon() 
    {
        HttpRequest request = new HttpRequest();
        AddChild(request);
        request.RequestCompleted += AuthRequestCompleted;

        AuthUserRequest message = new AuthUserRequest()
        {
            SteamTicket = "bannana"
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = request.Request("http://localhost:5000/auth", new string[]{"Content-Type: application/json"}, HttpClient.Method.Get, messageContent);
    }

    private void AuthRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if(responseCode != 200)
        {
            MD.Log("daymn..we got : " + responseCode); 
        }
        MD.Log(headers.ToString());
        var json = UTF8Encoding.UTF8.GetString(body);
        GD.Print(json);
        var messageData = JsonConvert.DeserializeObject<AuthUserResponse>(json); 
        GD.Print(messageData);        
    }
}