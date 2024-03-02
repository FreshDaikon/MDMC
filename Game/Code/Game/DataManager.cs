using System.Collections.Generic;
using Godot;
using System.Linq;

namespace Daikon.Game;

public partial class DataManager : Node
{
    public static DataManager Instance;

    [Export(PropertyHint.Dir)]
    private string _libraryPath;
    private List<DataObject> _library;

    public override void _Ready()
    {
        if(Instance != null)
        {
            Free();
        }
        Instance = this;
        _library = new List<DataObject>();
        SetupLibrary();
    }

    private void SetupLibrary()
    {
        GetObjectsInDirectory(_libraryPath);
        GD.Print(
            "Finished filling up library! =>  Library Items:");
        _library.ForEach(x => { GD.Print(
            "---------------------------      Library item: " + x.Name ); });
    }

    private void GetObjectsInDirectory(string path)
    {
        var dir = DirAccess.Open(path);
        if (dir == null) return;
        dir.ListDirBegin();
        var file = dir.GetNext();
        while(file != "")
        {
            if(dir.CurrentIsDir())
            {
                GetObjectsInDirectory(path + "/" + file);
                file = dir.GetNext();
            }
            else
            {
                var res = ResourceLoader.Load(path + "/" + file.Replace(".remap", ""));
                _library.Add((DataObject)res);
                file = dir.GetNext();
            }
        }
        dir.ListDirEnd();
    }

    public List<SkillData> GetAllSkills()
    {
        var skills = _library.Where(skill => skill is SkillData).Cast<SkillData>().ToList();
        return skills;
    }

    public SkillData GetSkill(int id)
    {
        var skill = GetAllSkills().Find(s => s.Id == id);
        return skill;
    }

    public Skill GetSkillInstance(int id)
    {
        var skill = GetAllSkills().Find(s => s.Id == id);
        var instance = skill.GetSkill();
        return instance;
    }

    public List<ArenaData> GetAllArenas()
    {
        var arenas = _library.Where(arena => arena is ArenaData).Cast<ArenaData>().ToList();
        return arenas;
    }
    
    public ArenaData GetArena(int id)
    {
        var arena = GetAllArenas().Find(s => s.Id == id);
        return arena;
    }

    public Arena GetArenaInstance(int id)
    {
        var arena = GetAllArenas().Find(a => a.Id == id);
        var instance = arena.GetArena();
        return instance;
    }

    public List<SkillContainerData> GetAllSkillContainers()
    {
        var skillContainers = _library.Where(container => container is SkillContainerData).Cast<SkillContainerData>().ToList();
        return skillContainers;
    }

    public SkillContainerData GetSkillContainer(int id)
    {
        var container = GetAllSkillContainers().Find(s => s.Id == id);
        return container;
    }

    public SkillContainer GetSkillContainerInstance(int id)
    {
        var container = GetAllSkillContainers().Find(s => s.Id == id);
        var instance = container.GetSkillContainer();
        return instance;
    }

    public List<ModifierData> GetAllModifiers()
    {
        var modifiers = _library.Where(mod => mod is ModifierData).Cast<ModifierData>().ToList();
        return modifiers;
    }

    public ModifierData GetModifier(int id)
    {
        var mod = GetAllModifiers().Find(m => m.Id == id);
        return mod;
    }

    public Modifier GetModifierInstance(int id)
    {
        var mod = GetAllModifiers().Find(m => m.Id == id);
        var instance = mod.GetModifier();
        return instance;
    }
}