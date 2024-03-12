using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Combat.Modifiers;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Entity.Components;

public partial class EntityModifiers : Node
{
    //Container:
    private Node _modContainer;
    private Entity _entity;

    public override void _Ready()
    {        
		SetProcess(GetMultiplayerAuthority() == 1);
        _modContainer = GetNode("%Mods");
        _entity = GetParent<Entity>();
        base._Ready();
    }

    public SkillResult AddModifier(Modifier mod)
    {
        if(!Multiplayer.IsServer())
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR }; ;
        if(mod != null)
        {
            var existingMod = _modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == mod.Data.Id);
            if(existingMod != null)
            {
                if(!existingMod.CanStack)
                {
                    return new SkillResult() { SUCCESS= false, result = MD.ActionResult.CANT_STACK };
                }             
                else
                {
                    if(existingMod.Stacks < existingMod.MaxStacks)
                    {
                        existingMod.Stacks += 1;
                        mod.Name = "mod_" + mod.Data.Id;
                        _modContainer.AddChild(mod);
                        Rpc(nameof(SyncMod), mod.Data.Id, true);
                        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
                    }
                    return new SkillResult() { SUCCESS = false, result = MD.ActionResult.MAX_STACKS};                    
                }   
            }
            else
            {
                mod.Name = "mod_" + mod.Data.Id;
                _modContainer.AddChild(mod);
                Rpc(nameof(SyncMod), mod.Data.Id, true);
                return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST }; 
            }            
        }
        else
        {
            GD.Print("Could not add mod...");
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };            
        }
    }

    public void RemoveModifier(int id)
    {
        var existingMod = _modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == id);
        if (existingMod == null) return;
        existingMod.QueueFree();
        if(Multiplayer.IsServer())
        {  
            Rpc(nameof(SyncMod), id, false);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncMod(int id, bool add)
    {
        if(add)
        {
            var newMod = DataManager.Instance.GetModifierInstance(id);
            if(newMod != null)
            {
                newMod.Name = "mod_" + id;
                _modContainer.AddChild(newMod);
            }
        }
        else
        {
            var existingMod = _modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == id);
            if(existingMod != null)
            {
                existingMod.QueueFree();
            }
        }
    }

    public List<Modifier> GetModifiers()
    {
        var mods = _modContainer.GetChildren().ToList();
        return mods.Count == 0 ? null : mods.Where(mod => mod is Modifier).Cast<Modifier>().ToList();
    }
}