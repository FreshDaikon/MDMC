using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Arena;

namespace Mdmc.Code.Game.Combat.SkillSystem.Targeting;

[GlobalClass]
public partial class AcquireAllInRangeOf : TargetAcquisition
{
    [Export] Mdmc.Code.Game.Combat.SkillSystem.SkillHandler Skill;
    [Export] public bool IncludeTarget;
    [Export] public WhatTarget Target;
    [Export] public TargetTypes TypesInRange;
    [Export] public float Range;
    [Export] public float InRange;

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
                null:
                IsInRange(player, player.CurrentTarget, Range) ?
                    player.CurrentTarget :
                    null,
            WhatTarget.None => null,
            _ => null
        };        
        if(target == null) return null;

        var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
        var all = ArenaManager.Instance.GetCurrentArena().GetEntities();

        var targets = TypesInRange switch
        {
            TargetTypes.Friendly => players?.Where(p => IsInRange(target, p, InRange)),
            TargetTypes.Enemy => enemies?.Where(e => IsInRange(target, e, InRange)),
            TargetTypes.All => all?.Where(a => IsInRange(target, a, InRange)),
            _ => null
        };
        if(targets is null) return null;
        return !targets.Where(t => t is not null).Any() ? null : targets.Where(x => x is not null).ToList();
    }
    
}