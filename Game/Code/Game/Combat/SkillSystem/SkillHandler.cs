using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Data;

namespace Mdmc.Code.Game.Combat.SkillSystem;

[GlobalClass]
public partial class SkillHandler: Node
{
    public enum SkillType
    {
        GCD,
        OGCD
    }

    [Export] private SkillType _type;
    [Export] private TimeController _timeController;
    [Export] private Mdmc.Code.Game.Combat.SkillSystem.ActionController _actionController;

    public PlayerArsenal Arsenal { get; private set; }
    public SkillData Data { get; private set; }
    public int AssignedSlot { get; private set; }

    // Initializers:
    public void SetArsenal(PlayerArsenal arsenal) => Arsenal = arsenal;
    
    public void SetData(SkillData data)
    { 
        Data = data;
    }
    public void AssignSlot(int slot) => AssignedSlot = slot;
    // Getters:
    public TimeController GetTimeInfo() => _timeController;
    public SkillType GetTypeInfo() => _type;


    public override void _Ready()
    {
        if(_timeController == null) return;
        if(_actionController == null) return;
        if(!Multiplayer.IsServer()) return;
        _actionController.ActionsTriggered += OnActionsTriggered;
    }

    public void OnActionsTriggered()
    {
        if(_timeController.CurrentCooldown > 0) { _timeController.StartCooldown(); }
    }

    public SkillResult CheckSkill()
    {
        if(_timeController.IsOnCooldown)
        {
            return new SkillResult(){ SUCCESS = false, result = Mdmc.Code.System.MD.ActionResult.ON_COOLDOWN };
        }
        return _actionController.CanActivate();
    }

    public SkillResult TriggerSkill()
    {
        if(!Multiplayer.IsServer()) return new SkillResult(){ SUCCESS = false, result = Mdmc.Code.System.MD.ActionResult.NOT_SERVER};
        var check = CheckSkill();
        if(!check.SUCCESS) return check;
        return _actionController.ActivateAction();
    }  
}