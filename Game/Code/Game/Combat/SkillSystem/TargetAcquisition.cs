using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity;

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

    public List<Entity> StoredTargets;
    public abstract List<Entity> GetTargets();

    public void StoreTargets()
    {
        StoredTargets = GetTargets();
    }

    public bool CanAcquireTargets()
    {
        if(!IsTargetRequired)
        {
            GD.Print("This acqisition does not need a target.");
            return true;  
        }  
        var targets = GetTargets();
        if(targets == null)    
        {
            GD.Print("Targets returned empty/null!");
            return false;
        }
        GD.Print("We got some Targets! [" + targets.Count +"]");
        return true; 
    }

    public bool IsInRange(Entity entityFrom, Entity entityTo, float Range)
    {
        var distance = (entityFrom.Controller.GlobalPosition - entityTo.Controller.GlobalPosition).Length();
        GD.Print("Distance:" + distance);
        GD.Print("Range :" + Range);
        return (entityFrom.Controller.GlobalPosition - entityTo.Controller.GlobalPosition).Length() <= Range;
    }
}