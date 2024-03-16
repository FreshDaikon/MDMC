using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Combat.ModifierSystem;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Entity.Components;

public partial class EntityModifiers : Node
{
    //Container:
    private Entity _entity;

    public override void _Ready()
    {        
        _entity = GetParent<Entity>();
    }

    public SkillResult AddModifier(ModifierHandler mod, Entity applier)
    {
        // Error if not server:
        if(!Multiplayer.IsServer()) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };
        // Error if the new mod is null:
        if(mod == null) return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };

        if(GetChildren().Count > 0 )
        {
            GD.Print("Already had some mods - let's see if it's the one we are adding:");
            var mods = GetChildren().Where(m => m is ModifierHandler).Cast<ModifierHandler>().ToList();
            var existingMod = mods.Count > 0 ? mods.Find(m => m.Data.Id == mod.Data.Id) : null;
            if(existingMod != null)
            {
                GD.Print("The mod already existed so try and add a stack.");
                if(existingMod.Stacks < existingMod.MaxStacks)
                {
                    existingMod.AddStack();
                    return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
                }             
                else
                {
                    return new SkillResult() { SUCCESS = false, result = MD.ActionResult.MAX_STACKS};                    
                }   
            }
            else
            {
                Rpc(nameof(SyncAddMod), mod.Data.Id, applier.Id);
                AddChild(mod);
                return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST }; 
            } 
        }
        else
        {
            Rpc(nameof(SyncAddMod), mod.Data.Id, applier.Id);
            AddChild(mod);
            return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST }; 
        }
    }

    public void RemoveModifier(int modId)
    {
        GD.Print("Remove mod!");
        var mods = GetChildren().Where(m => m is ModifierHandler).Cast<ModifierHandler>().ToList();
        var existingMod = mods.Count > 0 ? mods.Find(m => m.Data.Id == modId) : null;
        existingMod?.QueueFree();
        if(existingMod != null)
        {
            Rpc(nameof(SyncRemoveMod), modId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncAddMod(int modId, int applierId)
    {
        var newMod = DataManager.Instance.GetModifierInstance(modId);
        var applier = ArenaManager.Instance.GetCurrentArena().GetEntity(applierId); 
        newMod.SetLiveData(_entity, applier);
        AddChild(newMod);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncRemoveMod(int modId)
    {
        GD.Print("Synced removal of mod!");
        var mods = GetChildren().Where(m => m is ModifierHandler).Cast<ModifierHandler>().ToList();
        var existingMod = mods.Count > 0 ? mods.Find(m => m.Data.Id == modId) : null;
        existingMod?.QueueFree();
    }

    public List<ModifierHandler> GetModifiers()
    {
        var mods = GetChildren().ToList();
        return mods.Count == 0 ? null : mods.Where(mod => mod is ModifierHandler).Cast<ModifierHandler>().ToList();
    }
}