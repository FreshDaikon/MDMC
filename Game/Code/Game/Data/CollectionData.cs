using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

[GlobalClass]
public partial class CollectionData: DataObject
{
    public enum CollectionTypes
    {
        All,
        Skill,
        SkillContainer,
        Arena,
        Adversaries,
        Modifiers,
        Collection,
    }

    [ExportGroup("Collection Data")]
    [Export]    
    public CollectionTypes Type { get; private set; } = CollectionTypes.All;
    [Export]
    public DataObject[] collection { get; private set; }

    public List<T> GetData<T>()
    {
        return collection.ToList().Where(a => a is T).Cast<T>().ToList();
    }
}  