using System;
using System.Collections.Generic;
using System.Text;
using Daikon.Client.Connect;
using Daikon.Contracts.Data;
using Daikon.Contracts.Games;
using Daikon.Contracts.Models;
using Daikon.Contracts.Users;
using Daikon.Helpers;
using Daikon.Server.Database.Models;
using Godot;
using Newtonsoft.Json;

namespace Daikon.Server.Database;

public partial class DaikonServerConnect: Node
{
    public static DaikonServerConnect Instance;   
    private string _baseLocal = "http://127.0.0.1:5000";
    private string _baseRemote = "http://103.45.246.143:5000";
    private string _baseActual = "";

    private string[] _baseHeader = {"Content-Type: application/json"};
    private string _daikonKey = "bananna";
    
    // Various Configs:
    private bool _connectOnline = false;

    //Session Data 
    private Guid _sessionToken;
    private string _joinCode;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _baseActual = _baseLocal;
        base._Ready();
    }

    public void SetToLocalHost()
    {
        _baseActual = _baseLocal;
    }

    public void SetToRemote()
    {
        _baseActual = _baseRemote;
    }

    public void DaikonSetArenaRecord(ServerArenaRecord record)
    {
        GD.Print("Try and set a record!");
        var recordRequest = new HttpRequest();
        AddChild(recordRequest);

        recordRequest.RequestCompleted += (result, responseCode, headers, body) =>
        {
            if(responseCode != 200)
            {
                GD.Print("Response was not 200 OK -  [" + responseCode + "]"); 
                GD.Print("Result : " + result);
                CleanUp(recordRequest);
                return;
            }
            ArenaRecordRequestComplete(body);
            CleanUp(recordRequest);
        };
        
        var message = new SetArenRecordRequest()
        {
            Id = record.ArenaId,
            Progress = record.Progress,
            Runtime = record.Runtime,
            Participants = record.Players.ToArray(),
            Session = Guid.NewGuid(),
            DateTime = record.Time,
            ServerKey = _daikonKey
        }; 
        var messageContent = JsonConvert.SerializeObject(message);
        var error = recordRequest.Request(_baseActual+"/data/SetArenaRecord", _baseHeader, HttpClient.Method.Post, messageContent);
    }


    private void ArenaRecordRequestComplete(byte[] body)
    {
        GD.Print("We did it!");
    }

    //___________________________ Utility ____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////
    private void CleanUp(Node request)
    {
        RemoveChild(request);
        request.QueueFree();
    }
}