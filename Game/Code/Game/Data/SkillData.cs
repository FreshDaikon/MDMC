using Godot;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public partial class SkillData : DataObject
{
    [ExportGroup("Skill")]
    [Export] private PackedScene SkillScene;
   
    // Methods that needs overwriting :
    public SkillHandler GetSkill()
    {
        var skill = SkillScene.Instantiate<SkillHandler>();
        return SkillScene.Instantiate<SkillHandler>();
    }
}