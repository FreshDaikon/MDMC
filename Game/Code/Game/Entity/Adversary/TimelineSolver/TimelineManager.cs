using System;
using Godot;

namespace Daikon.Game;

public partial class TimelineManager : Node
{
    private AnimationPlayer behaviorPlayer;
    private string[] phases;
    private string currentPhase;

    private bool isEngaged = false;
    //Entity Reference:
    private AdversaryEntity entity;

    public bool IsEngaged { get { return isEngaged;} }
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
        if(!isEngaged)
            return;
        currentPhase = phases[0];
        Entity.Reset();
        behaviorPlayer.Stop();
    }

    public void Engage()
    {
        if(!Multiplayer.IsServer()) 
            return;
        if(isEngaged)
            return;

        isEngaged = true;
        CombatManager.Instance.StartCombat();
        behaviorPlayer.Play(currentPhase);
    }

    public void TriggerNextPhase()
    {
        if(!Multiplayer.IsServer()) 
            return;
        currentPhase = phases[Array.IndexOf(phases, currentPhase) + 1];
        behaviorPlayer.Play(currentPhase);        
    }
}
