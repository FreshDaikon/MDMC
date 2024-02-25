using System.Collections.Generic;
using System.Linq;
using Godot;
using SmartFormat;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class SkillContainer : Node
{
    //Serialized Properties:
    [ExportCategory("Serialzied Properties :")]
    [Export]
    public ComboSlot[] ComboSlots {get; set;}
    public int NextComboSlot = 0;   
    //Internals:
    public PlayerEntity Player;
    public SkillContainerObject Data;
    private Node skillSlotContainer;

    public override void _Ready()
    {
        skillSlotContainer = GetNodeOrNull("%SkillSlots");        
        InitializeContainer(); 
        base._Ready();
    }    

    public void ResetContainer()
    {        
        NextComboSlot = 0;
        foreach(var slot in GetSkillSlots())
        {
            slot.ResetSkill();
        }
    }
    public void InitializeContainer()
    {
        Player = GetParent().GetParent<PlayerArsenal>().Player;
        foreach(var slot in GetSkillSlots())
        {
            slot.Player = Player;
        }
    }

    public void SetSkill(int id, int slot)
    {        
        var skillSlot = (SkillSlot)skillSlotContainer.GetChild(slot);    
        skillSlot.SetSkill(id);
    }

    public List<SkillSlot> GetSkillSlots()
    {
        if(skillSlotContainer.GetChildCount() == 0)
            return null;
        return skillSlotContainer.GetChildren().Where(x => x is SkillSlot).Cast<SkillSlot>().ToList();
    }

    public Skill GetSkill(int index)
    {
        if(GetSkillSlots() != null)
        {
            return GetSkillSlots()[index].GetSkill();
        }
        else 
        {
            return null;
        }
    }

    public bool IsSkillOGCD(int slot)
    {
        var skillSlot = GetSkillSlots()[slot];
        if(skillSlot != null)
        {
            var skill = skillSlot.GetSkill();
            if(skill != null)
            {
                if(skill.TimerType == MD.SkillTimerType.OGCD)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public SkillResult TriggerSkill(int slot)
    {
        // Check for various things related to the contianer.
        var skillSlot = GetSkillSlots()[slot];
        if(skillSlot != null)
        {
            var skill = skillSlot.GetSkill();
            if(skill != null)
            {
                if(ComboSlots != null)
                {
                    if(ComboSlots.Length > 0)
                    {
                        var slotList = ComboSlots.ToList().Cast<ComboSlot>().ToList();
                        var slotFind = slotList.Find(x => x.ComboSlotIndex == slot);
                        if(slotFind != null)
                        {
                            if(slotList.IndexOf(slotFind) == NextComboSlot)
                            {
                                MD.Log("We did trigger a combo!");
                                NextComboSlot++;
                                Rpc(nameof(SyncNextCombo), NextComboSlot);
                                NextComboSlot = Mathf.Wrap(NextComboSlot, 0, ComboSlots.Length);
                                //Only apply the bonus if the skill is not a universal skill
                                if(!skill.IsUniversalSkill)
                                {
                                    int oldPotency = skill.AdjustedPotency;
                                    skill.AdjustedPotency = (int)(skill.AdjustedPotency * slotFind.ComboPotecyBonus);
                                    var adjustedSkillResult = skillSlot.TriggerSkill();  
                                    skill.AdjustedPotency = oldPotency;
                                    return adjustedSkillResult;

                                }   
                                var skillResult = skillSlot.TriggerSkill();                                          
                                return skillResult;
                            }
                            else
                            {
                                MD.Log("We failed the combo!");
                                NextComboSlot = 0;
                                Rpc(nameof(SyncNextCombo), NextComboSlot);
                                int oldPotency = skill.AdjustedPotency;
                                skill.AdjustedPotency = (int)(skill.AdjustedPotency * slotFind.FailurePotencyPenalty);
                                var skillResult = skillSlot.TriggerSkill();
                                skill.AdjustedPotency = oldPotency;
                                return skillResult;
                            }
                        }
                        else
                        {
                            //trigger skill normally at no penalty
                            return skillSlot.TriggerSkill();
                        }

                    }
                }                
                return skillSlot.TriggerSkill();
            }
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
        }
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncNextCombo(int index)
    {
        NextComboSlot = index;
    }

    
}