using System.Collections.Generic;
using Godot;
using Mdmc.Code.System;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
using PlayFab;

namespace Mdmc.Code.Server.PlayFab;

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
            ServerManager.Instance.StartServer(8080, 8, arena);
        }
        else
        {
            GD.Print("Init Local Server Without Arena..");
            ServerManager.Instance.StartServer(8080, 8, "-2009199945");
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
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
            GetTree().Quit();
        }); 

        if(GameserverSDK.ReadyForPlayers())
        {
            // Start Server :
            IDictionary<string, string> config = GameserverSDK.getConfigSettings();
            GD.Print("Config? :" + config.ToString());
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
                ServerManager.Instance.StartServer(listeningPort, 8,  cookie);
            }
            else
            {
                GD.Print("Start Playfab on [ " + listeningPort + " ]");
                GD.Print("Failed to get the cookie for this game..");
                ServerManager.Instance.StartServer(listeningPort, 8, "-2009199945");
            }
        }        
    }
}