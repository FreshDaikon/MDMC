using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Mdmc.Code.Game.Entity;
using Mdmc.Code.Game.Entity.Player;
using PlayFab.AdminModels;


[GlobalClass]
public partial class AcquireDirect : TargetAcquisition
{
    [Export] SkillHandler Skill;
    [Export] public WhatTarget Target;
    [Export] public float Range;    

    public override List<Entity> GetTargets()
    {
        PlayerEntity player = Skill.Arsenal.Player;
        var target = Target switch
        {
            WhatTarget.Player => player,
            WhatTarget.CurrentFriendlyTarget => player.CurrentFriendlyTarget == null ?
                player :
                IsInRange(player, player.CurrentFriendlyTarget, Range) ? 
                player.CurrentFriendlyTarget :
                null,
            WhatTarget.CurrentEnemyTarget => player.CurrentTarget == null ?
                null :
                IsInRange(player, player.CurrentTarget, Range) ?
                player.CurrentTarget :
                null,
            WhatTarget.None => null,
            _ => null
        };
        if(target == null) return null;
        var singleList = new List<Entity>
        {
            target
        };
        return singleList;
    }
    
}