using System.Collections.Generic;
using Godot;
using Daikon.System;

namespace Daikon.Game;

public partial class EntityController : CharacterBody3D
{
    public List<Vector3> Forces = new List<Vector3>();
    // Gravity Should Apply to all Entities:
    private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    [Export]
    private MultiplayerSynchronizer synchronizer;
    private ulong lastUpdate = 0;
    [Export]
    public Vector3 SyncPosition; 
    [Export]
    public Vector3 SyncRotation;

    private const int interpolationOffset = 100;
    private List<EntityState> entityStatesBuffer = new();
    private EntityState lastState;

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {
            MoveAndSlide();
            Rpc(nameof(UpdateEntityState),  (int)GameManager.Instance.ServerTick, Position, Rotation);
        }        
        else
        {
            UpdateController();
        }
    }
    private void UpdateController()
    {  
        if(GameManager.Instance.ServerTick == 0)
        { 
            return;
        }
        var safeTime = (int)GameManager.Instance.ServerTick;
        var renderTime = safeTime - interpolationOffset; 
        if(entityStatesBuffer.Count > 1)
        {
            while(entityStatesBuffer.Count > 2 && renderTime > entityStatesBuffer[1].TimeStamp)
            {
                entityStatesBuffer.RemoveAt(0);
            }
            var current = renderTime - entityStatesBuffer[0].TimeStamp;
            var difference = entityStatesBuffer[1].TimeStamp - entityStatesBuffer[0].TimeStamp;           
            var interpolationFactor = (float)current / (float)difference;
            if(current < 0)
            {
                return;
            }
            var newPosition = entityStatesBuffer[0].Position.Lerp(entityStatesBuffer[1].Position, interpolationFactor);
            var newRotation = entityStatesBuffer[0].Rotation.Lerp(entityStatesBuffer[1].Rotation, interpolationFactor);
            Position = newPosition;
            Rotation = newRotation;
        } 
    }
    public void Rotate(Vector2 direction)
    {
        if(!Multiplayer.IsServer())
            return;
        Rotation = new Vector3(0f, direction.Angle(), 0f);
    }
    public void Teleport(Vector3 destination)
    {
        if(Multiplayer.IsServer())
            return;
        Position = destination;
    }
    //RPC Calls:
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateEntityState(int time, Vector3 postition, Vector3 rotation)
    {
        var newState = new EntityState()
        {
          TimeStamp = time,
          Position = postition,   
          Rotation = rotation
        };
        if(lastState == null)
        {
            lastState = newState;
        }
        else if(newState.TimeStamp > lastState.TimeStamp)
        {
            lastState = newState;
            entityStatesBuffer.Add(newState);
        }
    }

}