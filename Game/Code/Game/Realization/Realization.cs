using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class Realization: Node3D
{    
    private enum Mode 
    {
        InPlace,
        InContainer,
        MoveTowards,
        ChaseTarget,
    }
    // Configuration:
    private Mode _mode = Mode.InPlace;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Node3D _container;
    private Node3D _targetNode;
    private float _lifetime;
    private float _speed;
    // Runtime:
    private double _startTime;
    private AnimationPlayer animationPlayer;
    public dynamic ExtraData;

    [Signal]
    public delegate void OnRealizationEndEventHandler();

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("%RealizationPlayer");
        animationPlayer.AnimationFinished += (animation) => Despawn(animation);
    }

    public override void _PhysicsProcess(double delta)
    {
        if(animationPlayer.CurrentAnimation == "End")
        {
            return;
        }
        var lapsed = (Time.GetTicksMsec() - _startTime) / 1000f;
        if(_mode == Mode.InPlace || _mode == Mode.InContainer)
        {
            //Simply track the lifetime, and kill it at the end.
            if(lapsed > _lifetime )
            {
                OnEndStart();
                animationPlayer.Play("End");
            }
        }
        else if(_mode == Mode.ChaseTarget)
        {
            if(_targetNode != null)
            {
                var lapsedMult = Mathf.Max(1f, lapsed/_lifetime);
                GlobalPosition = GlobalPosition.MoveToward(_targetNode.GlobalPosition, (float)(_speed * lapsedMult * (float)delta));
                var distance = GlobalPosition.DistanceSquaredTo(_targetNode.GlobalPosition);
                if(distance <= 0.01f )
                {
                    OnEndStart();
                    animationPlayer.Play("End");
                }       
            }
        }   
        else if(_mode == Mode.MoveTowards)
        {
            var lapsedMult = Mathf.Max(1f, lapsed/_lifetime);
            GlobalPosition = GlobalPosition.MoveToward(_endPosition, (float)(_speed * lapsedMult * (float)delta));
            var distance = Position.DistanceSquaredTo(_endPosition);
            if(distance <= 0.01f )
            {
                OnEndStart();
                animationPlayer.Play("End");
            }
        }
    }

    /// <summary>
    /// Simply Spawn Realization at some position (Global Position Expected)
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="lifetime"></param>
    public void Spawn(Vector3 startPosition, float lifetime, dynamic extraData = null)
    {
        //Set Data just in case:
        ExtraData = extraData;
        ResolveExtraData();

        _mode = Mode.InPlace;
        _startTime = Time.GetTicksMsec();
        ArenaManager.Instance.GetRealizationPool()?.AddChild(this);  
        GlobalPosition = startPosition;
        animationPlayer.Play("Spawn");    
    }
    /// <summary>
    /// Spawn the Realization within a Node3D with an offset.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="lifetime"></param>
    /// <param name="offset"></param>
    public void Spawn(Node3D container, Vector3 offset, float lifetime, dynamic extraData = null)
    {
        //Set Data just in case:
        ExtraData = extraData;
        ResolveExtraData();

        _mode = Mode.InContainer;        
        container.AddChild(this);
        Position = offset;
        _lifetime = lifetime;
        animationPlayer.Play("Spawn");
    }
    /// <summary>
    /// Spawn a Realization that will move linearly towards a position (Globals Expected)
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <param name="lifetime"></param>
    public void Spawn(Vector3 startPosition, Vector3 endPosition, float speed, dynamic extraData = null)
    {
        //Set Data just in case:
        ExtraData = extraData; 
        ResolveExtraData();

        _mode = Mode.MoveTowards;
        ArenaManager.Instance.GetRealizationPool()?.AddChild(this);
        GlobalPosition = startPosition;
        _endPosition = endPosition;
        _speed = speed;
        animationPlayer.Play("Spawn");
    }
    /// <summary>
    /// Spawn a realization that will move towards a target node3D (dynamic)
    /// </summary>
    /// <param name="target"> Target to chase </param>
    /// <param name="startPosition"> Initial Position </param>
    /// <param name="speed"> Movement Speed of the realization </param>
    /// <param name="lifetime"> Liftime of the realization </param>
    public void Spawn(Vector3 startPosition, Node3D target, float speed, float lifetime, dynamic extraData = null)
    {
        //Set Data just in case:
        ExtraData = extraData;
        ResolveExtraData();

        _mode = Mode.ChaseTarget;
        _startTime = Time.GetTicksMsec();
        _lifetime = lifetime;
        //Setup chase:

        ArenaManager.Instance.GetRealizationPool()?.AddChild(this);
        GlobalPosition = startPosition;
        _targetNode = target;
        _speed = speed;
        animationPlayer.Play("Spawn");
    }

    public virtual void ResolveExtraData(){ }

    public virtual void OnEndStart()
    {
        EmitSignal(nameof(OnRealizationEnd));
    }
    
    public void Despawn(string animation)
    {        
        if(animation == "End")
        {
            QueueFree();
        }        
    }
    public void Kill()
    {   
        QueueFree();
    }
}