using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;
using Mdmc.Code.Game.Data;
using Mdmc.Code.Game.Data.Decorators;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem;

public partial class SkillContainer : Node
{
    // Temp :  
    public SkillData[] Skills;
    public SkillContainerComponentData[] Components;
    public List<ModifierData> BuffsGranted;
    public MD.ContainerSlot AssignedSlot = 0;
    public SkillContainerData Data;
    // Extra 
    public bool GeneratesResource = false;
    public int MaxResource = 0;
    
    //Internals:
    public PlayerEntity Player { get; private set; }    
    public PlayerArsenal Arsenal { get; private set; }

    private int _currentResourceAmount = 0;

    // Signals :
    [Signal] public delegate void ContainerTriggeredEventHandler(SkillContainer container, Skill skill);

    public override void _Ready()
    {
        InitializeContainer(); 
        var components = GetChildren().Where(c => c is ContainerComponent).Cast<ContainerComponent>().ToList();
        
        foreach(var comp in components)
        {
            comp.SetupComponent();
        }
        base._Ready();
    }    

    public void CleanUp()
    {
        foreach(var buff in BuffsGranted)
        {
            // This really only works if all the mods are unique - so keep that in mind!
            Player.Modifiers.RemoveModifier(buff.Id);
        }
    }

    public void IncreaseResource(int amount)
    { 
        GD.Print("Resource increased! Total Resource is:" + _currentResourceAmount );
        _currentResourceAmount = Mathf.Clamp(_currentResourceAmount + amount, 0, MaxResource);
    }
    
    public void DecreaseResource(int amount)
    {
        _currentResourceAmount = Mathf.Clamp(_currentResourceAmount - amount, 0, MaxResource);
    }

    public int GetCurrentResourceAmount()
    {
        return _currentResourceAmount;
    }

    public void InitializeContainer()
    {
        Player = GetParent<PlayerArsenal>().Player;
        Arsenal = GetParent<PlayerArsenal>();
        
        var skillList = Skills.ToList();

        for(int i = 0; i < Skills.Length; i++)
        {
            GD.Print("Adding Skill..");
            var newSkill = DataManager.Instance.GetSkillInstance(Skills[i].Id);
            if(newSkill == null)
            { 
                GD.PrintErr("Can't init container skill was null..");
                return;
            }
            newSkill.AssignedSlot = i;
            newSkill.ParentContainer = this;
            newSkill.AssignedContainerSlot = AssignedSlot;
            newSkill.Name = "Skill_" + Skills[i].Name + "_" + i;
            newSkill.Player = Player;        
            AddChild(newSkill);
            newSkill.SkillTriggered += OnSkillTriggered;
            newSkill.InitSkill();
        }

        var counter = 0;
        foreach(var comp in Components)
        {
            var newComponent = comp.GetComponent();
            newComponent.Name = "Component_" + counter;
            newComponent.Container = this;
            AddChild(newComponent);
            counter ++;
        }

        // Only apply mods on server!
        if (!Multiplayer.IsServer()) return;
        
        foreach (var mod in BuffsGranted.Select(buff => buff.GetModifier()))
        {
            mod.InitData(Player, Player);
            Player.Modifiers.AddModifier(mod);
        }        
    }

    public Skill GetSkill(int slot)
    {
        if (GetChildCount() <= 0) return null;
        var current = GetChildren().Where(s => s is Skill).Cast<Skill>().ToList().Find(x => x.AssignedSlot == slot);
        return current;
    }

    public SkillResult TriggerSkill(int slot)
    {
        // Proceed to trigger whatever is in the current slot.
        var skill = GetSkill(slot);

        if (skill == null) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };

        var ruleMods = Player.Modifiers.GetModifiers().Where(m => m.Type == Modifiers.Modifier.ModType.RuleMod).ToList();
        foreach(var mod in ruleMods)
        {
            mod.HandleSkillTrigger(skill);
        } 
        var result = skill.TriggerSkill();

        if (!result.SUCCESS) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };

        EmitSignal(SignalName.ContainerTriggered, this, skill);
        return result;

    }

    public virtual void OnSkillTriggered(Skill skill){}

    public void UpdateComponentStates()
    {
        var components = GetChildren().Where(c => c is ContainerComponent).Cast<ContainerComponent>().ToList();
        
        foreach(var comp in components)
        {
            GD.Print("Update Component!");
            comp.UpdateComponent();
        }
    }

}