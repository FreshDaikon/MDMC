using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat;


[GlobalClass]
public abstract partial class ActionController : Node 
{
    [Export] protected SkillHandler _skill;
    [ExportCategory("Optional")]
    [Export] protected PackedScene _realizationScene;

    [Signal] public delegate void ActionsTriggeredEventHandler();
    public List<SkillAction> skillActions { get; private set; }
    
    public override void _Ready()
    {
        skillActions = GetChildren().Where(a => a is SkillAction).Cast<SkillAction>().ToList();
    }

    public abstract SkillResult ActivateAction();

    public abstract SkillResult CanActivate();
}