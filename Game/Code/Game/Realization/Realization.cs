using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class Realization: Node3D
{    
    // Configuration:
    protected AnimationPlayer animationPlayer;

    [Signal]
    public delegate void OnRealizationEndEventHandler();

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("%RealizationPlayer");
        animationPlayer.AnimationFinished += (animation) => Despawn(animation);
    }

    
    public virtual void Spawn(){}


    protected void OnEndStart()
    {
        EmitSignal(nameof(OnRealizationEnd));
    }
    
    protected void Despawn(string animation)
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