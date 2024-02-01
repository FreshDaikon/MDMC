using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

    //Keep Listing:
    private List<SkillContainer> skillContainers;
    private List<Skill> skills;
    private List<Modifier> modifiers;

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