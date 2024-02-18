using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class EntityModifiers : Node
{
    [Export(PropertyHint.Dir)]
    private string modsPath;
    //Spawner:
    private MultiplayerSpawner modSpawner;
    //Container:
    private Node modContainer;
    private Entity owner;

    public override void _Ready()
    {        
		SetProcess(GetMultiplayerAuthority() == 1);
        modSpawner = GetNode<MultiplayerSpawner>("%ModSpawner");
        modContainer = GetNode("%Mods");
        owner = GetParent<Entity>();
        GetModifierPaths();
        base._Ready();
    }

    public SkillResult AddModifier(Modifier mod)
    {
        if(mod != null)
        {
            var existingMod = modContainer.GetChildren().Where(m => m is Modifier).Cast<Modifier>().ToList().Find( m => m.Id == mod.Id);
            if(existingMod != null)
            {
                if(!existingMod.CanStack)
                {
                    MD.Log("This Mod doesn't stack!");
                    return new SkillResult() { SUCCESS= false, result = MD.ActionResult.CANT_STACK };
                }             
                else
                {
                    if(existingMod.Stacks < existingMod.MaxStacks)
                    {
                        existingMod.Stacks += 1;
                        mod.Name = "MOD_" + modContainer.GetChildCount();
                        mod.targetStatus = owner.Status;
                        modContainer.AddChild(mod);
                        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
                    }
                    return new SkillResult() { SUCCESS = false, result = MD.ActionResult.MAX_STACKS};                    
                }   
            }
            else
            {
                mod.Name = "MOD_" + modContainer.GetChildCount();
                mod.targetStatus = owner.Status;
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

    private void GetModifierPaths()
    {
        using var dir = DirAccess.Open(modsPath);
        if(dir != null)
        {
            MD.Log("Adding spawnable paths to ModifierSpawner");
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                modSpawner.AddSpawnableScene(modsPath + "/" + file.Replace(".remap", ""));
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }        
    }
}