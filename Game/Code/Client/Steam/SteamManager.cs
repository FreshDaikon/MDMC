using Godot;
using Steamworks;

public partial class SteamManager : Node
{
    public static SteamManager Instance { get; set; }
    private static uint appId { get; set; } = 2828690;

    public SteamManager()
    {
        if(Instance == null)
        {
            Instance = this;
            try
            {
                SteamClient.Init(appId, true);
                if(!SteamClient.IsValid)
                {
                    GD.Print("Something went wrong...");
                }
                else
                {
                    GD.Print("Steam successfully initialized!");
                    GD.Print("Welcome " + SteamClient.Name);
                    GD.Print("Overlay: " + SteamUtils.IsOverlayEnabled.ToString());                    
                }

            }
            catch (System.Exception e)
            {
                OS.Alert(("Something failed when initializing Steam..: " + e.Message)  , "STEAM");
                throw;
            }
        }
        else
        {
            GD.Print("Somehow a intsance already exists.");
        }
    }

    public override void _ExitTree()
    {
        SteamClient.Shutdown();
        base._ExitTree();
    }
}