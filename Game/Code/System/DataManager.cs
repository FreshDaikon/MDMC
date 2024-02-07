using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;

public partial class DataManager : Node
{
    public static DataManager Instance;

    [Export]
    private string SkillContainersPath;
    [Export]
    private string SkillsPath;
    [Export]
    private string ModifiersPath;
    [Export]
    private string ArenasPath;

    //Keep Listing:
    private List<SkillContainer> skillContainers;
    private List<Skill> skills;
    private List<Modifier> modifiers;
    private List<Arena> arenas;

    // id and path...
    private Dictionary<int, PackedScene> ArenaLookup;

    public override void _Ready()
    {
        if(Instance != null)
        {
            Free();
        }
        Instance = this;
        SetupModifiers();
        SetupSkillContainers();
        SetupSkills();
        SetupArenas();
    }

    private void SetupModifiers()
    {
        modifiers = new List<Modifier>();
        using var dir = DirAccess.Open(ModifiersPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                var res = (PackedScene)ResourceLoader.Load(ModifiersPath + "/" + file.Replace(".remap", ""));
                var instance = (Modifier)res.Instantiate();
                modifiers.Add(instance);
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
    private void SetupSkillContainers()
    {
        skillContainers = new List<SkillContainer>();
        using var dir = DirAccess.Open(SkillContainersPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                var res = (PackedScene)ResourceLoader.Load(SkillContainersPath + "/" + file.Replace(".remap", ""));
                var instance = (SkillContainer)res.Instantiate();   
                skillContainers.Add(instance);
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
    private void SetupSkills()
    {
        skills = new List<Skill>();
        using var dir = DirAccess.Open(SkillsPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                var res = (PackedScene)ResourceLoader.Load(SkillsPath + "/" + file.Replace(".remap", ""));
                var instance = (Skill)res.Instantiate();
                skills.Add(instance);
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }

    private void SetupArenas()
    {
        arenas = new List<Arena>();
        ArenaLookup = new Dictionary<int, PackedScene>();
        using var dir = DirAccess.Open(ArenasPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                var res = (PackedScene)ResourceLoader.Load(ArenasPath + "/" + file.Replace(".remap", ""));
                var instance = (Arena)res.Instantiate();
                var id = instance.Id;
                ArenaLookup.Add(id, res);
                instance.QueueFree();
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
    public Modifier GetModifier(int id)
    {
        var mod = modifiers.Find(a => a.Id == id);
        return mod == null ? null : (Modifier)mod.Duplicate();
    }
    public List<SkillContainer> GetSkillContainers()
    {
        return skillContainers;
    }
    public List<Skill> GetSkills()
    {
        return skills;
    }
    public SkillContainer GetSkillContainer(int id)
    {
        var container = skillContainers.Find(a => a.Id == id);
        return container == null ? null : (SkillContainer)container.Duplicate();
    }
    public Skill GetSkill(int id)
    {
        var skill = skills.Find(a => a.Id == id);
        return skill == null ? null : (Skill)skill.Duplicate();
    }    
    public Arena GetArena(int id)
    {
        PackedScene scene;
        if(ArenaLookup.TryGetValue(id, out scene))
        {
            return scene.Instantiate<Arena>();
        }
        else
        {
            return null;
        }
    }
    public Modifier GetModifierFromPath(string path)
    {
        var res = (PackedScene)ResourceLoader.Load(path.Replace(".remap", ""));
        var mod = (Modifier)res.Instantiate();
        return mod;
    }
    public RealizationObject GetRealizationObjectFromPath(string path)
    {
        var res = (PackedScene)ResourceLoader.Load(path.Replace(".remap", ""));
        var obj = res.Instantiate<RealizationObject>();
        return obj;
    }
}