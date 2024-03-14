using System;
using System.Collections.Generic;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Entity.Adversary;
using Mdmc.Code.Game.Entity.Adversary.Components;
using Mdmc.Code.Game.Realization;
using Mdmc.Code.System;


[GlobalClass]
public partial class ApplyThreat : SkillAction
{
    [Export] private SkillHandler _skill;
    [ExportCategory("Acquisition should only apply adversaries!")]
    [Export] private TargetAcquisition _acquisition;
    [ExportCategory("Threat Value - Can be negative!")]
    [Export] private int _threat;

    public override bool CanTrigger()
    {
        GD.Print("try and get some targets!");
        if(_acquisition.CanAcquireTargets())
        {
            GD.Print("Got some targets!");
            _acquisition.StoreTargets();
        }
        return _acquisition.CanAcquireTargets();
    }

    public override void TriggerAction()
    {
        var player = _skill.Arsenal.Player;
        var targets = _acquisition.StoredTargets;
        foreach(var entity in targets)
        {
            var adversaryStatus = entity.Status as AdversaryStatus;
            Rpc(nameof(RealizeAction), Int32.Parse(entity.Name), _threat);
        }
    }
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeAction(int target, int value)
    {
        if(_realizationScene == null ) return;
        
        var entity = ArenaManager.Instance.GetCurrentArena().GetEntity(target);
        
        if(entity == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_realizationScene, 3)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .WithTarget(entity.Controller, 10)
            .Spawn();      
        real.OnRealizationEnd += () => {
            RealizationManager.Instance.SpawnDamageNumber(value, entity.Controller.GlobalPosition, MD.GetSkillTypeColor(MD.SkillType.TANK));
        };      
    }
}