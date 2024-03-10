using Godot;

namespace Mdmc.Code.Game.Realization;

public partial class Realization: Node3D
{    
    // Configuration:
   
    protected float Lifetime;
    protected ulong StartTime;

    [Signal]
    public delegate void OnRealizationEndEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        var lapsed = (Time.GetTicksMsec() - StartTime) / 1000f;
        if (!(lapsed > Lifetime)) return;
        EmitSignal(SignalName.OnRealizationEnd);
        Despawn();
    }

    public virtual void Spawn(){}
   
    protected void Despawn()
    {        
        QueueFree();
    }
    public void Kill()
    {   
        QueueFree();
    }
}