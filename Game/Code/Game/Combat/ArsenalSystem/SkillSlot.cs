using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem;

public partial class SkillSlot : Node
{
    //Serialized Properties:
    [ExportGroup("Skill Slot Properties")]
    [Export]
    public MD.SkillType SlotSkillType { get; set; }
    [Export]
    public float PotencyMultiplier = 1.0f;
    [Export]
    public float ThreatMultiplier = 1.0f;
    //Useful Setters:
    public PlayerEntity Player;
    private Node skillHolder;

    public override void _Ready()
    {
        skillHolder = GetNode("%SkillHolder");
        base._Ready();
    }

    public SkillResult TriggerSkill()
    {
        var skill = skillHolder.GetChildOrNull<Skill>(0);
        if(skill != null)
        {
            return skill.TriggerSkill();
        }
        else
        {
            return new SkillResult()
            {
                SUCCESS = false,
                result = MD.ActionResult.ERROR
            };
        }
    }
    public void ResetSkill()
    {
        var skill = skillHolder.GetChildOrNull<Skill>(0);
        skill?.Reset();
    }
    public Skill GetSkill()
    {
        if(skillHolder.GetChildCount() > 0)
        {
            return (Skill)skillHolder.GetChild(0);
        }
        return null;
    }

    public void SetSkill(int id)
    {
        var newSkill = DataManager.Instance.GetSkillInstance(id);
        if(newSkill == null)
        {
            return;
        }
        if(skillHolder.GetChildCount() > 0)
        {
            var current = (Skill)skillHolder.GetChild(0);
            current?.Free();
        }
        newSkill.Name = "Skill_" + 1;
        newSkill.SkillType = SlotSkillType;
        newSkill.Player = Player == null ? null : Player;
        if(!newSkill.IsUniversalSkill)
        {
            newSkill.AdjustedPotency = (int)(newSkill.BasePotency * PotencyMultiplier);
        }             
        skillHolder.AddChild(newSkill);
        newSkill.InitSkill();
        
    }
}