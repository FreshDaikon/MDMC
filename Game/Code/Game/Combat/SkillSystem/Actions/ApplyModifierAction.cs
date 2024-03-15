using System;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Data;
using Mdmc.Code.Game.RealizationSystem;

namespace Mdmc.Code.Game.Combat.SkillSystem.Actions;

[GlobalClass]
public partial class ApplyModifierAction : SkillAction
{
    [Export] private SkillHandler _skill;
    [Export] private TargetAcquisition _acquisition;
    [Export] private ModifierData _modifierData;

    public override bool CanTrigger()
    {
        if(_acquisition.CanAcquireTargets())
        {
            _acquisition.StoreTargets();
        }
        return _acquisition.CanAcquireTargets();
    }

    public override void TriggerAction()
    {
        var player = _skill.Arsenal.Player;
        var targets = _acquisition.StoredTargets;
        if(targets.Count > 0)
        {
            foreach(var entity in targets)
            {
                var mod = DataManager.Instance.GetModifierInstance(_modifierData.Id);
                Rpc(nameof(RealizeAction), Int32.Parse(entity.Name));
                entity.Modifiers.AddModifier(mod);
            }
        }
        GD.Print(targets.ToString());
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeAction(int target)
    {
        if(_realizationScene == null ) return;
        
        var entity = ArenaManager.Instance.GetCurrentArena().GetEntity(target);
        
        if(entity == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_realizationScene, 3)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .WithTarget(entity.Controller, 10)
            .Spawn();      
    }    
}