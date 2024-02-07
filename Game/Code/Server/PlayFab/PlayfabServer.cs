using Godot;
using PlayFab;
using Microsoft.Playfab.Gaming.GSDK;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
using System.Collections.Generic;

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
        var arguments = new Godot.Collections.Dictionary();
        foreach (var argument in OS.GetCmdlineArgs())
        {
            if(argument.Contains("--playfab"))
            {
                MD.Log("Init Playfab Instance..");
                Running = true;
                StartAsPlayfabInstance();
                return;
            }
        }
        MD.Log("Start Standalone.."); 
        StartAsStandalone();
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
            MD.Log("Maintenance shutdown in : " + time.ToString());
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
            MD.Log("Start Playfab on [ " + listeningPort + " ]");
            if(ServerManager.Instance.StartAsPlayfabServer(cookie, listeningPort, 8))
            {
                if(GameserverSDK.ReadyForPlayers())
                {
                    MD.Log("Server Success!");
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
            MD.Log("Start Playfab on [ " + listeningPort + " ]");
            ServerManager.Instance.StartAsPlayfabServer("-1", listeningPort, 8);
            if(GameserverSDK.ReadyForPlayers())
            {
                MD.Log("Server Success!");
                return;
            }
            else 
            {
                GameserverSDK.LogMessage("Server could not be started..shutdown");
                GetTree().Quit();
            }
        }
    }


    private void StartAsStandalone()
    {
        ServerManager.Instance.StartAsStandaloneServer(8080, 8);
    }
}