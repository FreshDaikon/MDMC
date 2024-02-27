using Godot;
using Daikon.Helpers;
using Daikon.Game;
using Daikon.Client.Connect;

namespace Daikon.Client;

public partial class ClientManager : Node3D
{	
    //Singleton Instance: 
    public static ClientManager Instance;
    public MD.InputScheme CurrentInputScheme { get; private set; } 

    public enum UIState
    {
        None,
        Frontend,
        Ingame,
        HUD,
    }
    //References & Denpendencies:
    // UI :
    [Export]
    private PackedScene frontendScene;
    [Export]
    private PackedScene hudScene;
    [Export]
    private PackedScene ingamemenuScene; 

    private UILandingPage frontend;
    private UIHUDMain hud;
    private UIIngameMenu ingameMenu;
    
    //Game Containers:
    private Node UIContainer;
    public UIState CurrentUIState = UIState.None;

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;
        GD.Print("Client Starting...");
        CallDeferred(nameof(InitClient));
        //Call when Ready:
    }

    private void InitClient()
    {
        // Get References:
        UIManager.Instance.TrySetUIState(UIManager.UIState.Frontend);
    }
    public override void _ExitTree()
    {
        //If We started a serve locally make sure to kill it.
        if(ClientMultiplayerManager.Instance.LocalPid != -1)
        {
            var error = OS.Kill(ClientMultiplayerManager.Instance.LocalPid);
            if(error != Error.Ok)
            {
                GD.Print("Could not kill Saved Server ID.");
            }
        }
        base._ExitTree();
    }

    public void ResetClient()
    {
        ArenaManager.Instance.UnloadArena();
        GameManager.Instance = null;
        CombatManager.Instance = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Input.GetLastMouseVelocity().Length() > 0f)
        {
            CurrentInputScheme = MD.InputScheme.KEYBOARD;
        }
        else if(Input.GetActionStrength("Move_Up") > 0f || Input.GetActionStrength("Move_Down") > 0f || Input.GetActionStrength("Move_Left") > 0f || Input.GetActionStrength("Move_Right") > 0f)
        {
            CurrentInputScheme = MD.InputScheme.GAMEPAD;
        }
        base._PhysicsProcess(delta);       
    }


}
