using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class EntityModifiers : Node
{
    //Container:
    private Node modContainer;
    private Entity entity;

    public override void _Ready()
    {        
		SetProcess(GetMultiplayerAuthority() == 1);
        modContainer = GetNode("%Mods");
        entity = GetParent<Entity>();
        base._Ready();
    }

    public SkillResult AddModifier(Modifier mod)
    {
        if(!Multiplayer.IsServer())
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR }; ;
        if(mod != null)
        {
            var existingMod = modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == mod.Data.Id);
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
                        mod.entity = entity;
                        modContainer.AddChild(mod);
                        Rpc(nameof(SyncMod), mod.Data.Id, true);
                        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
                    }
                    return new SkillResult() { SUCCESS = false, result = MD.ActionResult.MAX_STACKS};                    
                }   
            }
            else
            {
                mod.Name = "mod_" + mod.Data.Id;
                mod.entity = entity;
                modContainer.AddChild(mod);
                Rpc(nameof(SyncMod), mod.Data.Id, true);
                return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST }; 
            }            
        }
        else
        {
            MD.Log("Could not add mod...");
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };            
        }
    }

    public void RemoveModifier(int id)
    {
        var existingMod = modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == id);
        if(existingMod != null)
        {
            existingMod.QueueFree();
            if(Multiplayer.IsServer())
            {  
                Rpc(nameof(SyncMod), id, false);
            }
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
                newMod.entity = entity;
                modContainer.AddChild(newMod);
            }
        }
        else
        {
            var existingMod = modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Data.Id == id);
            if(existingMod != null)
            {
                existingMod.QueueFree();
            }
        }
    }

    public List<Modifier> GetModifiers()
    {
        var mods = modContainer.GetChildren()
            .Where(mod => mod is Modifier)
            .Cast<Modifier>()
            .ToList();        
        return mods;
    }
}