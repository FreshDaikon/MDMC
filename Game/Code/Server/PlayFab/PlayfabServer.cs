using Godot;
using PlayFab;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
using System.Collections.Generic;
using Daikon.Helpers;

namespace Daikon.Server;

public partial class PlayfabServer : Node
{

    public static PlayfabServer instance;
    public string ListeningPortKey = "game_port";    
    private string arena = "None";
    
    public bool Running = false;
    public override void _Ready()
    {
        if(instance != null)
        {
            Free();
            return;
        }
        instance = this;
        PlayFabSettings.staticSettings.TitleId="5FDDE";    
        CallDeferred(nameof(CheckCMD));
    }

    private void CheckCMD()
    {
        var args = MD.GetArgs();
        if(args.ContainsKey("playfab"))
        {
            GD.Print("Init Playfab Instance..");
            Running = true;
            StartAsPlayfabInstance();
            return;
        }
        else if(args.ContainsKey("arena"))
        {
            GD.Print("Init Local Server with arena..");
            var arena = args["arena"];
            ServerManager.Instance.StartAsStandaloneServer(8080, 8, arena);
        }
        else
        {
            GD.Print("Init Local Server Without Arena..");
            ServerManager.Instance.StartAsStandaloneServer(8080, 8);
        }
    }

    private void StartAsPlayfabInstance()
    {
        try
        {
            GameserverSDK.Start();
        }
        catch (GSDKInitializationException initEx)
        {
            GD.Print("Cannot start GSDK. Please make sure the MPAgent is running. ", false);
            GD.Print($"Got Exception: {initEx.ToString()}", false);
            return;
        }   

        GameserverSDK.RegisterHealthCallback(() => {
            return true;
        });
        GameserverSDK.RegisterShutdownCallback(() => {
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
            GetTree().Quit();
        }); 
        GameserverSDK.RegisterMaintenanceCallback((time) => {
            GD.Print("Maintenance shutdown in : " + time.ToString());
            // Just shut down right now... whatever.
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
            GetTree().Quit();
        }); 
        IDictionary<string, string> config = GameserverSDK.getConfigSettings();
        int listeningPort = 8080;
        if(config?.ContainsKey(ListeningPortKey) == true)
        {
            GD.Print("Parse that port!");
            listeningPort = int.Parse(config[ListeningPortKey]);
        }
        if(config.TryGetValue(GameserverSDK.SessionCookieKey, out var cookie))
        {
            GD.Print("Start Playfab on [ " + listeningPort + " ]");
            GD.Print("Session Cookie was: " + cookie);
            if(ServerManager.Instance.StartAsPlayfabServer(cookie, listeningPort, 8))
            {
                if(GameserverSDK.ReadyForPlayers())
                {
                    GD.Print(MD.Runtime.PlayfabServer, "", "Server Success!");
                    return;
                }
                else 
                {
                    GameserverSDK.LogMessage("Server could not be started..shutdown");
                    GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
                    GetTree().Quit();
                }
            }
            else
            {
                GameserverSDK.LogMessage("Server could not be started..shutdown");
                GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
                GetTree().Quit();
            }  
        }
        else
        {
            GD.Print("Start Playfab on [ " + listeningPort + " ]");
            ServerManager.Instance.StartAsPlayfabServer("-1", listeningPort, 8);
            if(GameserverSDK.ReadyForPlayers())
            {
                GD.Print("Server Success!");
                return;
            }
            else 
            {
                GameserverSDK.LogMessage("Server could not be started..shutdown");
                GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
                GetTree().Quit();
            }
        }
    }

}