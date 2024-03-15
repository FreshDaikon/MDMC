using System.Collections.Generic;
using Godot;

namespace Mdmc.Code.Game.Combat.SkillSystem;

[GlobalClass]
public abstract partial class TargetAcquisition: Node
{    
    public enum WhatTarget
    {
        Player,
        CurrentFriendlyTarget,
        CurrentEnemyTarget,
        None,
    }

    public enum TargetTypes
    { 
        Friendly,
        Enemy,
        Neutral,
        All        
    }

    [Export] public bool IsTargetRequired { get; private set;}

    public List<Entity.Entity> StoredTargets;
    public abstract List<Entity.Entity> GetTargets();

    public void StoreTargets()
    {
        StoredTargets = GetTargets();
    }

    public bool CanAcquireTargets()
    {
        if(!IsTargetRequired)
        {
            return true;  
        }  
        var targets = GetTargets();
        if(targets == null)    
        {
            return false;
        }
        return true; 
    }

    public bool IsInRange(Entity.Entity entityFrom, Entity.Entity entityTo, float Range)
    {
        return (entityFrom.Controller.GlobalPosition - entityTo.Controller.GlobalPosition).Length() <= Range;
    }
}