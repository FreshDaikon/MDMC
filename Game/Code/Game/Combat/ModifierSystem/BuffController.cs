using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Mdmc.Code.Game.Combat.ModifierSystem;

[GlobalClass]
public partial class BuffController: Node
{
    [Export] protected ModifierHandler _modifier;    
    public List<ModifierBuff> Buffs { get; private set; } = new();

    public override void _Ready()
    {
        Buffs = GetChildren().Where(m => m is ModifierBuff).Cast<ModifierBuff>().ToList();
    }
}