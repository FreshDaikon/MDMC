using System;
using Godot;
using Steamworks;

namespace Mdmc.Code.Client.Steam;

public partial class SteamManager : Node
{
    public static SteamManager Instance;
    private static uint _appId { get; } = 2828690;

    public bool IsSteamRunning = false;

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

    public bool InitSteam()
    {
        OS.SetEnvironment("SteamAppID", _appId.ToString());
        OS.SetEnvironment("SteamAppName", _appId.ToString());
        try
        {
            SteamClient.Init(_appId, true);
        }
        catch(Exception e)
        {
            GD.Print("Steam failed.. :" + e.Message);
            return false;
        }
        if(!SteamClient.IsValid)
        {
            GD.Print("Could not init steam.. continue.");
            return false;
        }
        else
        {
            GD.Print("Steam Initialized!"); 
            GD.Print("Welcome " + SteamClient.Name);
            GD.Print("Overlay: " + SteamUtils.IsOverlayEnabled.ToString());    
            return true;
        }
    }

    public override void _ExitTree()
    {
        if(IsSteamRunning)
        {
            SteamClient.Shutdown(); 
        }
        base._ExitTree();
    }
}