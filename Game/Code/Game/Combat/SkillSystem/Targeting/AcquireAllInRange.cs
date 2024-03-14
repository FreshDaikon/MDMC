using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity;
using Mdmc.Code.Game.Entity.Player;


[GlobalClass]
public partial class AcquireAllInRange : TargetAcquisition
{
    [Export] SkillHandler Skill;
    [Export] public bool IncludePlayer;
    [Export] public TargetTypes Types;
    [Export] public float Range;

    public override List<Entity> GetTargets()
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
        if(targets.Where(t => t is not null).Count() == 0 ) return null;
        return targets.Where(x => x is not null).ToList();
    }
    
}