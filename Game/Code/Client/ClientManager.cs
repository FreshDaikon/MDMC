using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.System;
using Mdmc.Code.Client.Multiplayer;

namespace Mdmc.Code.Client;

public partial class ClientManager : Node3D
{	
    public static ClientManager Instance;
    public MD.InputScheme CurrentInputScheme { get; private set; } 

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
    }

    private static void InitClient()
    {
        UI.UIManager.Instance.TrySetUIState(UI.UIManager.UIState.Frontend);
    }
    
    public static void ResetClient()
    {
        ArenaManager.Instance.UnloadArena();
        GameManager.Instance = null;
        CombatManager.Instance = null;
    }
    
    public override void _ExitTree()
    {
        //If We started a server locally make sure to kill it.
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
