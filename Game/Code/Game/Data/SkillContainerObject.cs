using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class SkillContainerObject : DataObject
{

    public SkillContainer GetSkillContainer()
    {
        var instance = Scene.Instantiate<SkillContainer>();
        instance.Data = this;
        return instance;
    }
}