using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class ClientManager : Node3D
{	
    public static ClientManager Instance;
    public string ConnectionType { get; private set;}
    private ENetMultiplayerPeer peer;
    public MD.InputScheme CurrentInputScheme { get; private set; } 

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;
        GD.Print("Client Starting");
        
    }
    public void StartAsClient(string IP, int PORT)
    {
            Engine.MaxFps = 120;
            GD.Print("Prepare - connect to server...");
            peer = new ENetMultiplayerPeer();
            var error = peer.CreateClient(IP, PORT);
            if(error != Error.Ok)
            {
                OS.Alert("Failed to start Client [" + error.ToString() +"]");
                return; 
            }
            Multiplayer.MultiplayerPeer = peer;
            GD.Print("Client Connected to Server! As : " + Multiplayer.MultiplayerPeer ); 
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

    public void TempStart(string ip, int port)
    {
        StartAsClient(ip, port);
    }
}
