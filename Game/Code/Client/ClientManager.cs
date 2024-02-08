using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

public partial class ClientManager : Node3D
{	
    //Singleton Instance: 
    public static ClientManager Instance;

    public MD.InputScheme CurrentInputScheme { get; private set; } 
    //References & Denpendencies:
    // UI :
    [Export(PropertyHint.File)]
    private string frontendPath;
    [Export(PropertyHint.File)]
    private string hudPath;
    [Export(PropertyHint.File)]
    private string ingamemenuPath;

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
    }

    private void InitClient()
    {
        // Get References:
        UIContainer = GetNode<Node>("%UIContainer");
        // Get Dependencies:
        // Frontend
        var frontendScene = (PackedScene)ResourceLoader.Load(frontendPath);
        frontend = frontendScene.Instantiate<UILandingPage>();
        // HUD
        var hudScene = (PackedScene)ResourceLoader.Load(hudPath);
        hud = hudScene.Instantiate<UIHUDMain>();

        // Steam Init:
        if(SteamManager.Instance.InitSteam())
        {
            //Steam was success.
            ToggleFrontend();
        }
    }

    public void ToggleHud()
    {
        Node hudContainer = UIContainer.GetNode("HUDContainer");
        if(hudContainer.GetNode(hud.Name.ToString()) != null)
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
        if(frontendContainer.GetNode(frontend.Name.ToString()) != null)
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
        if(ingameContainer.GetNode(ingameMenu.Name.ToString()) != null)
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
