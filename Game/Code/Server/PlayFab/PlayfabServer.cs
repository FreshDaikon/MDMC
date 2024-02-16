using Godot;
using PlayFab;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
using System.Collections.Generic;
using Daikon.System;

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
            MD.Log(MD.Runtime.PlayfabServer, "", "Init Playfab Instance..");
            Running = true;
            StartAsPlayfabInstance();
            return;
        }
        else if(args.ContainsKey("arena"))
        {
            MD.Log(MD.Runtime.PlayfabServer, "", "Init Local Server with arena..");
            var arena = args["arena"];
            ServerManager.Instance.StartAsStandaloneServer(8080, 8, arena);
        }
        else
        {
            MD.Log(MD.Runtime.PlayfabServer, "", "Init Local Server Without Arena..");
            ServerManager.Instance.StartAsStandaloneServer(8080, 8);
        }
    }

    private void StartAsPlayfabInstance()
    {
        //Start GameServer:
        try
        {
            GameserverSDK.Start();
        }
        catch (GSDKInitializationException initEx)
        {
            GD.Print("Cannot start GSDK. Please make sure the MockAgent is running. ", false);
            GD.Print($"Got Exception: {initEx.ToString()}", false);
            return;
        }   

        GameserverSDK.RegisterHealthCallback(() => {
            return true;
        });
        GameserverSDK.RegisterShutdownCallback(() => {
            GetTree().Quit();
        }); 
        GameserverSDK.RegisterMaintenanceCallback((time) => {
            MD.Log(MD.Runtime.PlayfabServer, "", "Maintenance shutdown in : " + time.ToString());
            // Just shut down right now... whatever.
            GetTree().Quit();
        }); 
        IDictionary<string, string> config = GameserverSDK.getConfigSettings();
        int listeningPort = 8080;
        if(config?.ContainsKey(ListeningPortKey) == true)
        {
            listeningPort = int.Parse(config[ListeningPortKey]);
        }
        string cookie;
        if(config.TryGetValue(GameserverSDK.SessionCookieKey, out cookie))
        {
            MD.Log(MD.Runtime.PlayfabServer, "", "Start Playfab on [ " + listeningPort + " ]");
            if(ServerManager.Instance.StartAsPlayfabServer(cookie, listeningPort, 8))
            {
                if(GameserverSDK.ReadyForPlayers())
                {
                    MD.Log(MD.Runtime.PlayfabServer, "", "Server Success!");
                    return;
                }
                else 
                {
                    GameserverSDK.LogMessage("Server could not be started..shutdown");
                    GetTree().Quit();
                }
            }
            else
            {
                GameserverSDK.LogMessage("Server could not be started..shutdown");
                GetTree().Quit();
            }  
        }
        else
        {
            MD.Log(MD.Runtime.PlayfabServer, "", "Start Playfab on [ " + listeningPort + " ]");
            ServerManager.Instance.StartAsPlayfabServer("-1", listeningPort, 8);
            if(GameserverSDK.ReadyForPlayers())
            {
                MD.Log(MD.Runtime.PlayfabServer, "", "Server Success!");
                return;
            }
            else 
            {
                GameserverSDK.LogMessage("Server could not be started..shutdown");
                GetTree().Quit();
            }
        }
    }

}