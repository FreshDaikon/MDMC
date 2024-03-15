using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Arena;

namespace Mdmc.Code.Game.Combat.SkillSystem.Targeting;

[GlobalClass]
public partial class AcquireAllInRange : TargetAcquisition
{
    [Export] Mdmc.Code.Game.Combat.SkillSystem.SkillHandler Skill;
    [Export] public bool IncludePlayer;
    [Export] public TargetTypes Types;
    [Export] public float Range;

    public override List<Entity.Entity> GetTargets()
    {
        var player = Skill.Arsenal.Player;
        var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
        var all = ArenaManager.Instance.GetCurrentArena().GetEntities();
        
        var targets = Types switch
        {
            TargetTypes.Friendly => players?.Where(p => IsInRange(player, p, Range)),
            TargetTypes.Enemy => enemies?.Where(e => IsInRange(player, e, Range)),
            TargetTypes.All => all?.Where(a => IsInRange(player, a, Range)),
            _ => null
        };
        if(targets is null) return null;
        return !targets.Where(t => t is not null).Any() ? null : targets.Where(x => x is not null).ToList();
    }
    
}