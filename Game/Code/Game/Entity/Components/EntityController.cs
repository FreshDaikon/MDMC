using System.Collections.Generic;
using Godot;
using Daikon.Helpers;

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

    public Vector3 SavedPosition;
    public Vector3 SavedRotation;

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
    public virtual void UpdateController()
    {  
        if(GameManager.Instance.ServerTick == 0)
        { 
            return;
        }
        var safeTime = (int)GameManager.Instance.ServerTick;
        var renderTime = safeTime - interpolationOffset; 
        if(entityStatesBuffer.Count > 1)
        {
            while(entityStatesBuffer.Count > 2 && renderTime > entityStatesBuffer[2].TimeStamp)
            {
                entityStatesBuffer.RemoveAt(0);
            } 
            if(entityStatesBuffer.Count > 2)
            {
                var current = renderTime - entityStatesBuffer[1].TimeStamp;
                var difference = entityStatesBuffer[2].TimeStamp - entityStatesBuffer[1].TimeStamp;           
                var interpolationFactor = (float)current / (float)difference;
                if(current < 0)
                {
                    return;
                }
                var newPosition = entityStatesBuffer[1].Position.Lerp(entityStatesBuffer[2].Position, interpolationFactor);
                var newRotation = entityStatesBuffer[1].Rotation.Lerp(entityStatesBuffer[2].Rotation, interpolationFactor);
                SavedPosition = newPosition;
                SavedRotation = newRotation;
                UpdatePosition();
                UpdateRotation();
            }
            else if(renderTime > entityStatesBuffer[1].TimeStamp)
            {
                var current = renderTime - entityStatesBuffer[0].TimeStamp;
                var difference = entityStatesBuffer[1].TimeStamp - entityStatesBuffer[0].TimeStamp;
                var extrapolationFactor = ((float)current / (float)difference) -1f;
                if(current < 0)
                {
                    return;
                }
                var positonDelta = (entityStatesBuffer[1].Position - entityStatesBuffer[0].Position);
                var newPosition = entityStatesBuffer[1].Position + positonDelta * extrapolationFactor;
                var newRotation = entityStatesBuffer[1].Rotation.Lerp(entityStatesBuffer[2].Rotation, extrapolationFactor);
                SavedPosition = newPosition;
                SavedPosition = newRotation;
                UpdatePosition();
                UpdateRotation();
            }
        } 
    }

    public virtual void UpdateRotation()
    {
        //implment on inheritors.
    }
    public virtual void UpdatePosition()
    {
        //Implement on inheritors.
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