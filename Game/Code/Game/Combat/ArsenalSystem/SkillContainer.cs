using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class SkillContainer : Node
{
    public enum ContainerType
    {
        Armament,
        Suit,
    }
    
    public SkillSlotData[] SkillSlots;
    public List<ModifierData> BuffsGranted;
    public float BaseGcd = 1.5f;
    public MD.ContainerSlot AssignedSlot = 0;
    //Internals:
    public PlayerEntity Player;
    public SkillContainerData Data;

    public override void _Ready()
    {
        InitializeContainer(); 
        base._Ready();
    }    

    public void CleanUp()
    {
        foreach(var buff in BuffsGranted)
        {
            Player.Modifiers.RemoveModifier(buff.Id);
        }
    }

    public void InitializeContainer()
    {
        Player = GetParent<PlayerArsenal>().Player;
        
        if (!Multiplayer.IsServer()) return;
        
        foreach (var mod in BuffsGranted.Select(buff => buff.GetModifier()))
        {
            Player.Modifiers.AddModifier(mod);
        }
    }

    public void SetSkill(int id, int slot)
    {        
        var newSkill = DataManager.Instance.GetSkillInstance(id);
        if(newSkill == null)
        {
            return;
        }
        if(GetChildCount() > 0)
        {
            var current = GetChildren().Where(s => s is Skill).Cast<Skill>().ToList().Find(x => x.AssignedSlot == slot);
            current?.Free();
        }
        // Get SkillSlot Data:
        var slotData = SkillSlots[slot];
        newSkill.AssignedContainerSlot = AssignedSlot;
        newSkill.AssignedSlot = slot;
        newSkill.Name = "Skill_" + slot;
        newSkill.SkillType = slotData.SlotSkillType;
        newSkill.Player = Player;
        if(!newSkill.IsUniversalSkill)
        {
            newSkill.AdjustedPotency = (int)(newSkill.BasePotency * slotData.PotencyMultiplier);
        }             
        AddChild(newSkill);
        newSkill.InitSkill();  
    }

    public void InitSkills()
    {
        if(GetChildCount() == 0) return;
        
        var skills = GetChildren().ToList().Where(c => c is Skill).Cast<Skill>().ToList();
        
        if (skills.Count <= 0) return;
        
        foreach(var skill in skills)
        {
            var slotData = SkillSlots[skills.IndexOf(skill)];
            skill.SkillType = slotData.SlotSkillType;
            skill.InitSkill();
            skill.Reset();
        }
    }

    public Skill GetSkill(int slot)
    {
        if (GetChildCount() <= 0) return null;
        var current = GetChildren().Where(s => s is Skill).Cast<Skill>().ToList().Find(x => x.AssignedSlot == slot);
        return current;
    }

    public bool IsSkillOGCD(int slot)
    {
        var skill = GetSkill(slot);
        if(skill !=  null)
        {
            return skill.TimerType == MD.SkillTimerType.OGCD;
        }
        return false;
    }

    public SkillResult TriggerSkill(int slot)
    {
        var skill = GetSkill(slot);
        var stack = Player.Arsenal.Stack;

        if (skill == null) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };
        
        var effects = stack.ResolveEffects(skill);
        GD.Print("Effects in stack:" + effects.Count);
        skill.Effects = effects;
            
        var result = skill.TriggerSkill();

        if (!result.SUCCESS) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };

        stack.LastSkill = skill;
        
        foreach (var rule in skill.Rules)
        {
            var newRule = rule.GetRule();
            newRule.Init(skill);
            stack.AddRule(newRule);
        }
        return result;
    }
}