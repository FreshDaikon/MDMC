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
                        mod.Name = "MOD_" + modContainer.GetChildCount();
                        mod.targetStatus = entity.Status;
                        modContainer.AddChild(mod);
                        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
                    }
                    return new SkillResult() { SUCCESS = false, result = MD.ActionResult.MAX_STACKS};                    
                }   
            }
            else
            {
                mod.Name = "MOD_" + modContainer.GetChildCount();
                mod.targetStatus = entity.Status;
                modContainer.AddChild(mod);
                return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST }; 
            }            
        }
        else
        {
            MD.Log("Could not add mod...");
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR };            
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