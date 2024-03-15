using System.Collections.Generic;
using Godot;

namespace Mdmc.Code.Game.Combat.SkillSystem.Targeting;

[GlobalClass]
public partial class AcquireDirect : TargetAcquisition
{
    [Export] Mdmc.Code.Game.Combat.SkillSystem.SkillHandler Skill;
    [Export] public WhatTarget Target;
    [Export] public float Range;    

    public override List<Entity.Entity> GetTargets()
    {
        var player = Skill.Arsenal.Player;
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
        var singleList = new List<Entity.Entity>
        {
            target
        };
        return singleList;
    }
    
}