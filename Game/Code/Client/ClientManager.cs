using Godot;
using Daikon.System;

namespace Daikon.Client;

public partial class ClientManager : Node3D
{	
    //Singleton Instance: 
    public static ClientManager Instance;

    public MD.InputScheme CurrentInputScheme { get; private set; } 
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

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;
        GD.Print("Client Starting...");
        //Call when Ready:
        CallDeferred(nameof(InitClient));
        
        #if CLIENT
        GD.Print("We are the client build!");
        #endif
    }

    private void InitClient()
    {
        // Get References:
        UIContainer = GetNode<Node>("%UIContainer");
        
        // Get Dependencies:
        // Frontend
        frontend = frontendScene.Instantiate<UILandingPage>();
        // HUD
        hud = hudScene.Instantiate<UIHUDMain>();
        // Ingame 
        ingameMenu = ingamemenuScene.Instantiate<UIIngameMenu>();
        // Steam Init:
        if(SteamManager.Instance.InitSteam())
        {
            //Steam was success.
            // TODO : Implement Steam Things.
        }
        ToggleFrontend();
    }

    public void ToggleHud()
    {
        Node hudContainer = UIContainer.GetNode("HUDContainer");
        if(hudContainer.GetNodeOrNull(hud.Name.ToString()) != null)
        {
            hudContainer.RemoveChild(hud);
        }
        else
        {
            hudContainer.AddChild(hud);
        }
    }

    public void ToggleFrontend()
    {
        Node frontendContainer = UIContainer.GetNode("FrontendContainer");
        if(frontendContainer.GetNodeOrNull(frontend.Name.ToString()) != null)
        {
            frontendContainer.RemoveChild(frontend);
        }
        else
        {
            frontendContainer.AddChild(frontend);
        }
    }

    public void ToggleIngameMenu()
    {
        Node ingameContainer = UIContainer.GetNode("IngameContainer");
        if(ingameContainer.GetNodeOrNull(ingameMenu.Name.ToString()) != null) 
        {
            ingameContainer.RemoveChild(ingameMenu);
        }
        else 
        {
            ingameContainer.AddChild(ingameMenu);
        }
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
