

using System;
using Godot;
using Godot.Collections;

public partial class EntityPositionSync : MultiplayerSynchronizer
{    
    [Export]
    public Godot.Collections.Array SyncState = new(){ 
        0,              // Frame
        Vector3.Zero,   // Position
        Vector3.Zero    // Rotation.
    };

    private enum Pos
    {
        Frame = 0,
        Position = 1,
        Rotation = 2,
    }

    private CharacterBody3D _controller;
    
    private double _currentTime = 0;
    private double _lastPacketTime = 0;
    private double _packetTime = 0;

    public override void _Ready()
    {
        Synchronized += OnSynchronized;
        _controller = GetNode<CharacterBody3D>("%Controller");
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer() && _controller != null)
        {
            _currentTime += delta;
            SyncState[0] = _currentTime;
            GetState();
        }
    }

    private void SetState() 
    {
        _controller.Position = (Vector3)SyncState[1];
        _controller.Rotation = (Vector3)SyncState[2];
    }

    private void GetState()
    {
        SyncState[1] = _controller.Position;
        SyncState[2] = _controller.Rotation;
    }

    private void OnSynchronized()
    {
        if(_controller == null)
            return;
        if(IsPreviousFrame())
            return;
        Correction();
        SetState();
    }

    private void Correction()
    {
        // var delta = controller.Position - (Vector3)SyncState[1];
        Vector3 synVec = (Vector3)SyncState[1];
        SyncState[1] = _controller.Position.Lerp(new Vector3(synVec.X, _controller.Position.Y, synVec.Z), 0.5f);
        Vector3 syncH = (Vector3)SyncState[1];
        if((_controller.Position.Y - synVec.Y) > 3) 
        {
            SyncState[1] = new Vector3(syncH.X, synVec.Y, syncH.Z);
        }
    }

    private bool IsPreviousFrame()
    {
        if((double)SyncState[0] < _lastPacketTime)
        {
            return true;
        }
        else
        {
            _lastPacketTime = (double)SyncState[0];
            return false;
        }
    }
}