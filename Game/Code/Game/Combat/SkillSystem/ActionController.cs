using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Mdmc.Code.Game.Combat.SkillSystem;

[GlobalClass]
public abstract partial class ActionController : Node 
{
    [Export] protected SkillHandler _skill;
    [ExportCategory("Optional")]
    [Export] protected PackedScene _realizationScene;

    [Signal] public delegate void ActionsTriggeredEventHandler();
    public List<SkillAction> SkillActions { get; private set; }
    
    public override void _Ready()
    {
        SkillActions = GetChildren().Where(a => a is SkillAction).Cast<SkillAction>().ToList();
    }

    public abstract SkillResult ActivateAction();

    public abstract SkillResult CanActivate();
}