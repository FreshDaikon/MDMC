using System.Linq;
using Godot;


public partial class RealizationObject: Node3D
{    
    [ExportGroup("Serialized Properties")]
    [Export]
    public float Lifetime = 1f;    
    [Export]
    public float Speed = 10f;
    [Export]
    public AnimationPlayer animationPlayer;
    [Export]
    public MD.RealizationMode realizationMode;

    [ExportGroup("Internal")]
    [Export]
    private DataID ID;
    public int Id
    {
        get { return ID.Id; }
    }

    public Vector3 _startPos;
    public Vector3 _targetPosition;
    public Node3D _target;
    public float _startTime;

    public override void _Ready()
    {
        animationPlayer.AnimationFinished += (animation) => Despawn(animation);
    }

    public override void _PhysicsProcess(double delta)
    {
        if(animationPlayer.CurrentAnimation == "End")
        {
            return;
        }

        var lapsed = (Time.GetTicksMsec() - _startTime) / 1000f;
        if(realizationMode == MD.RealizationMode.STATIC)
        {
            //Simply track the lifetime, and end it at the end.
            if(lapsed > Lifetime )
            {
                OnEndStart();
                animationPlayer.Play("End");
            }
        }
        else if(realizationMode == MD.RealizationMode.DYNAMIC)
        {
            if(_target != null)
            {
                var lapsedMult = Mathf.Max(1f, lapsed/Lifetime);
                Position = Position.MoveToward(_target.Position, (Speed * lapsedMult) * (float)delta);
                var distance = Position.DistanceSquaredTo(_target.Position);
                if(distance <= 0.01f )
                {
                    OnEndStart();
                    animationPlayer.Play("End");
                }       
            }
        }   
    }
    public virtual void Spawn(Vector3 worldPos)
    {
        realizationMode = MD.RealizationMode.STATIC;

        Position = worldPos;
        _startTime = Time.GetTicksMsec();
        GameManager.Instance.GetRealizationPool().AddChild(this);
        animationPlayer.Play("Spawn");    
    }
    public virtual void SpawnInTransform(Node3D transform, Vector3 offset)
    {
        realizationMode = MD.RealizationMode.STATIC;
        Position = offset;
        _startTime = Time.GetTicksMsec();
        transform.AddChild(this);
        animationPlayer.Play("Spawn");
    }
    //Default Behavior can be overwritten
    public virtual void SpawnWithTarget(Node3D target, Vector3 startPosition)
    {        
        realizationMode = MD.RealizationMode.DYNAMIC;
        Position = startPosition;
        _startPos = startPosition;
        _target = target;
        _startTime = Time.GetTicksMsec();
        GameManager.Instance.GetRealizationPool().AddChild(this);
        animationPlayer.Play("Spawn");
    }

    public virtual void OnEndStart()
    {
        //Implement this in instances different from this one.
    }
    
    public virtual void Despawn(string animation)
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