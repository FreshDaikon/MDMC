using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Combat.Modifiers;
using Mdmc.Code.Game.Combat.SkillSystem;
using Mdmc.Code.Game.Data;

namespace Mdmc.Code.Game;

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

    public List<T> GetData<T>()
    {
        return _library.Where(t => t is T).Cast<T>().ToList();
    }

    public SkillHandler GetSkillInstance(int id)
    {
        var skills = GetData<SkillData>();
        var skill = skills.Find(s => s.Id == id);
        var instance = skill.GetSkill();
        return instance;
    }

    public ArenaInstance GetArenaInstance(int id)
    {
        var arena = GetData<ArenaData>().Find(a => a.Id == id);
        var instance = arena.GetArena();
        return instance;
    }

    public SkillContainer GetSkillContainerInstance(int id)
    {
        var container = GetData<SkillContainerData>().Find(s => s.Id == id);
        var instance = container.GetSkillContainer();
        return instance;
    }

    public Modifier GetModifierInstance(int id)
    {
        var mod = GetData<ModifierData>().Find(m => m.Id == id);
        var instance = mod.GetModifier();
        return instance;
    }
}