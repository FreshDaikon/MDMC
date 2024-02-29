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
    
    private EntityState _lastState;
    public Vector3 SavedPosition;
    public Vector3 SavedRotation;

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {
            MoveAndSlide();
            //Rpc(nameof(UpdateEntityState), GameManager.Instance.GameClock, Position, Rotation);
        }        
        else
        {
            MoveAndSlide();
           // UpdateController();       
        }
    }

    public virtual void UpdateController()
    {  
        if(!GameManager.Instance.IsGameRunning())
        { 
            return;
        }        
        var renderTime = GameManager.Instance.GameClock - InterpolationOffset;
        if(_entityStatesBuffer.Count > 2)
        {
            while(_entityStatesBuffer.Count > 2 && renderTime > _entityStatesBuffer[2].TimeStamp)
            {
                _entityStatesBuffer.RemoveAt(0);
            } 
            if(_entityStatesBuffer.Count > 2)
            {
                var current = renderTime - _entityStatesBuffer[1].TimeStamp;
                var difference = _entityStatesBuffer[2].TimeStamp - _entityStatesBuffer[1].TimeStamp;           
                var interpolationFactor = (float)current / (float)difference;
                if(current < 0)
                {
                    return;
                }
                var newPosition = _entityStatesBuffer[1].Position.Lerp(_entityStatesBuffer[2].Position, interpolationFactor);
                var newRotation = _entityStatesBuffer[1].Rotation.Lerp(_entityStatesBuffer[2].Rotation, interpolationFactor);
                SavedPosition = newPosition;
                SavedRotation = newRotation;
                UpdatePosition();
                UpdateRotation();
            }
            else if(renderTime > _entityStatesBuffer[1].TimeStamp)
            {
                var current = renderTime - _entityStatesBuffer[0].TimeStamp;
                var difference = _entityStatesBuffer[1].TimeStamp - _entityStatesBuffer[0].TimeStamp;
                var extrapolationFactor = ((float)current / (float)difference) -1f;
                if(current < 0)
                {
                    return;
                }
                var positonDelta = _entityStatesBuffer[1].Position - _entityStatesBuffer[0].Position;
                var rotationDelta = _entityStatesBuffer[1].Rotation - _entityStatesBuffer[0].Rotation;                
                var newPosition = _entityStatesBuffer[1].Position + positonDelta * extrapolationFactor;
                var newRotation = _entityStatesBuffer[1].Rotation + rotationDelta * extrapolationFactor;
                SavedPosition = newPosition;
                SavedRotation = newRotation;
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
          TimeStamp = time,
          Position = postition,   
          Rotation = rotation
        };
        if(_lastState == null)
        {
            _lastState = newState;
        }
        else if(newState.TimeStamp > _lastState.TimeStamp)
        {
            _lastState = newState;
            _entityStatesBuffer.Add(newState);
        }
    }

}