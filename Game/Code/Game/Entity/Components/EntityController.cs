using System.Collections.Generic;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class EntityController : CharacterBody3D
{
    public List<Vector3> Forces = new List<Vector3>();
    private float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private const double InterpolationOffset = 0.1;

    private List<EntityState> _entityStatesBuffer = new();
    private List<EntityState> _localStatesBuffer = new();
    
    public EntityState LatestState;
    public Vector3 SavedPosition;
    public Vector3 SavedRotation;

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {
            MoveAndSlide();
            Rpc(nameof(UpdateEntityState), GameManager.Instance.GameClock, Position, Rotation);
        }        
        else
        {
            MoveAndSlide();
            var newState = new EntityState()
            {
                timeStamp = GameManager.Instance.GameClock,
                position = Position,   
                rotation = Rotation
            };
            UpdateController();       
        }
    }

    public void UpdateController()
    {  
        if(!GameManager.Instance.IsGameRunning())
        { 
            return;
        }        
        var renderTime = GameManager.Instance.GameClock - Mathf.Clamp(GameManager.Instance.GetLatency(), 0.02, InterpolationOffset);

        if(_entityStatesBuffer.Count > 1)
        {
            while(_entityStatesBuffer.Count > 2 && renderTime > _entityStatesBuffer[1].timeStamp)
            {
                _entityStatesBuffer.RemoveAt(0);
            } 
            var current = renderTime - _entityStatesBuffer[0].timeStamp;
            var difference = _entityStatesBuffer[1].timeStamp - _entityStatesBuffer[0].timeStamp;           
            var interpolationFactor = (float)current / (float)difference;
            if(current < 0)
            {
                return;
            }
            var newPosition = _entityStatesBuffer[0].position.Lerp(_entityStatesBuffer[1].position, interpolationFactor);
            var newRotation = _entityStatesBuffer[0].rotation.Lerp(_entityStatesBuffer[1].rotation, interpolationFactor);
            SavedPosition = newPosition;
            SavedRotation = newRotation;
            UpdatePosition();
            UpdateRotation();
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
        if(!Multiplayer.IsServer())
            return;
        Position = destination;
    }
    //RPC Calls:
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 10)]
    private void UpdateEntityState(double time, Vector3 postition, Vector3 rotation)
    {
        var newState = new EntityState()
        {
          timeStamp = time,
          position = postition,   
          rotation = rotation
        };
        if(LatestState == null)
        {
            LatestState = newState;
        }
        else if(newState.timeStamp > LatestState.timeStamp)
        {
            LatestState = newState;
            _entityStatesBuffer.Add(newState);
        }
    }

}