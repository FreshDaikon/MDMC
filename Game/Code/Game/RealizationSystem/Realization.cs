using Godot;

namespace Mdmc.Code.Game.RealizationSystem;

public partial class Realization: Node3D
{    
    // Configuration:   
    public PackedScene Scene { get; set; }
    public Node3D TargetObject { get; set; }
    public float Lifetime { get; set; }
    public double StartTime { get; set; } 
    public Vector3 StartPosition { get; set; }
    public Vector3 Offset { get; set; }
    public Vector3 TargetPosition { get; set; }
    public float Speed { get; set; }
    public Vector3 Size { get; set; }
    public float Radius { get; set; }
    public Node3D RootTransform { get; set; }
    public Vector3 LookatTarget { get; set; }


    [Signal]
    public delegate void OnRealizationEndEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        if(TargetObject != null)
        {
            // Take care of chasing target:
            var lapsed = (Time.GetTicksMsec() - StartTime) / 1000f;                   
            var lapsedMultiplier = Mathf.Max(1f, (float)(lapsed / Lifetime));
            GlobalPosition = GlobalPosition.MoveToward(TargetObject.GlobalPosition, (float)(Speed * lapsedMultiplier * (float)delta));
            var distance = GlobalPosition.DistanceSquaredTo(TargetObject.GlobalPosition);            
            if (!(distance <= 0.01f)) return;
            EmitSignal(SignalName.OnRealizationEnd);
            Despawn();
        }
        else if(TargetPosition != Vector3.Zero)
        {
            // Take care of chasing target:
            var lapsed = (Time.GetTicksMsec() - StartTime) / 1000f;                   
            var lapsedMultiplier = Mathf.Max(1f, (float)(lapsed / Lifetime));
            GlobalPosition = GlobalPosition.MoveToward(TargetPosition, (float)(Speed * lapsedMultiplier * (float)delta));
            var distance = GlobalPosition.DistanceSquaredTo(TargetPosition);            
            if (!(distance <= 0.01f)) return;
            EmitSignal(SignalName.OnRealizationEnd);
            Despawn();
        }
        else
        {   
            // Take care to just wait out the lifetime.
            var lapsed = (Time.GetTicksMsec() - StartTime) / 1000f;
            if (!(lapsed > Lifetime)) return;
            EmitSignal(SignalName.OnRealizationEnd);
            Despawn();
        }
    }

    public void SpawnRealization()
    {
        StartTime = Time.GetTicksMsec();
        //Add the realization base to the correct object:
        if(RootTransform != null)
        {
            RootTransform.AddChild(this);
            Position = StartPosition;
            if(Offset != Vector3.Zero)
            {
                Position += Offset;
            }
        }
        else
        {
            RealizationManager.Instance.AddRealization(this);
            GlobalPosition = StartPosition;
            if(Offset != Vector3.Zero)
            {
                Position += Offset;
            }
        }
        // Add the Scene:
        if (Scene.Instantiate() is not RealizerTypes.BaseRealizer newScene) return;
        newScene.ParentRealization = this;
        AddChild(newScene);
        newScene.CallDeferred(nameof(newScene.Initialize));
    }  
    protected void Despawn()
    {        
        QueueFree();
    }
    public void Kill()
    {   
        QueueFree();
    }
}