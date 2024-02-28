using System;
using Godot;

namespace Daikon.Game;

public partial class TimelineManager : Node
{
    private AnimationPlayer behaviorPlayer;
    private string[] phases;
    private string currentPhase;
    //Entity Reference:
    private AdversaryEntity entity;
    public AdversaryEntity Entity { get { return entity; }}

    public override void _Ready()
    {
        behaviorPlayer = GetNode<AnimationPlayer>("%BehaviorPlayer");
        entity = GetParent<AdversaryEntity>();
        phases = behaviorPlayer.GetAnimationList();
        GD.Print(phases);
        currentPhase = phases[0];
    }

    public void Reset()
    {
        if(!Multiplayer.IsServer()) 
            return;
        currentPhase = phases[0];
        Entity.Reset();
        behaviorPlayer.Stop();
    }

    public void Stop()
    {
        if(!Multiplayer.IsServer())
            return;
        currentPhase = phases[0];
        behaviorPlayer.Stop();
    }

    public void Start()
    {
        if(!Multiplayer.IsServer()) 
            return;
        behaviorPlayer.Play(currentPhase);
    }

    public void TriggerNextPhase()
    {
        if(!Multiplayer.IsServer()) 
            return;
        currentPhase = phases[Array.IndexOf(phases, currentPhase) + 1];
        behaviorPlayer.Play(currentPhase);        
    }

    public bool GetState()
    {
        return behaviorPlayer.IsPlaying();
    }
}
