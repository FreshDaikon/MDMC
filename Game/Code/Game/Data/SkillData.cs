using Godot;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public partial class SkillData : DataObject
{
    [ExportGroup("Skill")]
    [Export] private PackedScene SkillScene;
   
    // Methods that needs overwriting :
    public Combat.SkillSystem.SkillHandler GetSkill()
    {
        var skill = SkillScene.Instantiate<Combat.SkillSystem.SkillHandler>();
        return SkillScene.Instantiate<Combat.SkillSystem.SkillHandler>();
    }
}