using Godot;
using Steamworks;

public partial class SteamManager : Node
{
    public static SteamManager Instance;

    private static uint appId { get; } = 2828690;

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
        OS.SetEnvironment("SteamAppID", appId.ToString());
        OS.SetEnvironment("SteamAppName", appId.ToString());
        try
        {
            SteamClient.Init(appId, true);
            if(!SteamClient.IsValid)
            {
                GD.Print("Something went wrong...");
                return false;
            }
            else
            {
                GD.Print("Steam successfully initialized!");
                GD.Print("Welcome " + SteamClient.Name);
                GD.Print("Overlay: " + SteamUtils.IsOverlayEnabled.ToString());    
                return true;                
            }
        }
        catch (System.Exception e)
        {
            OS.Alert(("Something failed when initializing Steam..: " + e.Message)  , "STEAM");
            throw;
        }
    }

    public override void _ExitTree()
    {
        SteamClient.Shutdown();
        base._ExitTree();
    }
}