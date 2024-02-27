using System.Collections.Generic;
using System.Linq;
using Godot;
using SmartFormat;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class SkillContainer : Node
{
    public SkillSlotData[] SkillSlots;
    public List<ModifierObject> BuffsGranted;

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

    public void ResetContainer()
    {        
        NextComboSlot = 0;
        /**
        TODO : reset skills:
        foreach(var slot in GetSkillSlots())
        {
            slot.ResetSkill();
        }
        **/
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
        if(skill != null)
        {
            var ComboSlots = SkillSlots.Where(x => x.IsComboSlot).ToList();
            if(ComboSlots != null)
            {
                if(ComboSlots.Count > 0)
                {
                    var slotFind = ComboSlots.Find(x => x.ComboSlotIndex == slot);
                    if(slotFind != null)
                    {
                        if(ComboSlots.IndexOf(slotFind) == NextComboSlot)
                        {
                            NextComboSlot++;
                            Rpc(nameof(SyncNextCombo), NextComboSlot);
                            NextComboSlot = Mathf.Wrap(NextComboSlot, 0, ComboSlots.Count);
                            if(!skill.IsUniversalSkill)
                            {
                                int oldPotency = skill.AdjustedPotency;
                                skill.AdjustedPotency = (int)(skill.AdjustedPotency * slotFind.ComboPotecyBonus);
                                var adjustedSkillResult = skill.TriggerSkill();  
                                skill.AdjustedPotency = oldPotency;
                                return adjustedSkillResult;

                            }   
                            var skillResult = skill.TriggerSkill();                                          
                            return skillResult;
                        }
                        else
                        {
                            MD.Log("We failed the combo!");
                            NextComboSlot = 0;
                            Rpc(nameof(SyncNextCombo), NextComboSlot);
                            int oldPotency = skill.AdjustedPotency;
                            skill.AdjustedPotency = (int)(skill.AdjustedPotency * slotFind.FailurePotencyPenalty);
                            var skillResult = skill.TriggerSkill();
                            skill.AdjustedPotency = oldPotency;
                            return skillResult;
                        }
                    }
                    else
                    {
                        //trigger skill normally at no penalty
                        return skill.TriggerSkill();
                    }

                }
            }                
            return skill.TriggerSkill();
        }
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncNextCombo(int index)
    {
        NextComboSlot = index;
    }    
}