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
    public List<ModifierObject> BuffsGranted;
    public float BaseGcd = 1.5f;
    public MD.ContainerSlot AssignedSlot = 0;
    public int NextComboSlot = 0;   
    //Internals:
    public PlayerEntity Player;
    public SkillContainerObject Data;
    private Node SkillHolder;
    private Node skillSlotContainer;

    public override void _Ready()
    {
        SkillHolder = GetNodeOrNull("%Skills");
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
        if(Multiplayer.IsServer())
        {
            foreach(var buff in BuffsGranted)
            {
                var mod = DataManager.Instance.GetModifierInstance(buff.Id);
                var result = Player.Modifiers.AddModifier(mod);
            }
        }
    }

    public void SetSkill(int id, int slot)
    {        
        var newSkill = DataManager.Instance.GetSkillInstance(id);
        if(newSkill == null)
        {
            return;
        }
        if(SkillHolder.GetChildCount() > 0)
        {
            var current = SkillHolder.GetChildren().Where(s => s is Skill).Cast<Skill>().ToList().Find(x => x.AssignedSlot == slot);
            current?.Free();
        }
        // Get SkillSlot Data:
        var slotData = SkillSlots[slot];

        newSkill.AssignedContainerSlot = AssignedSlot;
        newSkill.AssignedSlot = slot;
        newSkill.Name = "Skill_" + slot;
        newSkill.SkillType = slotData.SlotSkillType;
        newSkill.Player = Player == null ? null : Player;
        if(!newSkill.IsUniversalSkill)
        {
            newSkill.AdjustedPotency = (int)(newSkill.BasePotency * slotData.PotencyMultiplier);
        }             
        SkillHolder.AddChild(newSkill);
        newSkill.InitSkill();  
    }

    public void InitSkills()
    {
        if(SkillHolder.GetChildCount() == 0) return;
        
        var skills = SkillHolder.GetChildren().ToList().Where(c => c is Skill).Cast<Skill>().ToList();
        if(skills.Count > 0)
        {
            foreach(var skill in skills)
            {
                var slotData = SkillSlots[skills.IndexOf(skill)];
                skill.SkillType = slotData.SlotSkillType;
                skill.InitSkill();
                skill.Reset();
            }
        }
    }

    public Skill GetSkill(int slot)
    {
        if(SkillHolder.GetChildCount() > 0)
        {
            var current = SkillHolder.GetChildren().Where(s => s is Skill).Cast<Skill>().ToList().Find(x => x.AssignedSlot == slot);
            return current;
        }
        return null;
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

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncNextCombo(int index)
    {
        NextComboSlot = index;
    }    
}