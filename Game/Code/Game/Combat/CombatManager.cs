using System;
using System.Collections.Generic;
using System.Linq;
using Daikon.Contracts.Models;
using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.Server.Database;
using Mdmc.Code.Server.Database.Models;
using Mdmc.Code.System;
using static System.Int32;

namespace Mdmc.Code.Game.Combat;

public partial class CombatManager: Node
{
    public static CombatManager Instance;   
    public List<CombatMessage> Messages = new List<CombatMessage>();
    public bool IsInCombat = false;

    //Combat Session Tracker:
    private double CombatStartTime = 0;

    public enum CombatState 
    {
        None,
        Running,
        Paused,
        Stopped
    }

    private List<EntityContributionTracker> _damage = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> _heal = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> _enmity = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> _effect = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> _deaths = new List<EntityContributionTracker>();

    private CombatState _currentState = CombatState.None;

    public CombatState CurrentState
    {
        get { return _currentState; }
    }

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        // Post a record when we win!
        CallDeferred(nameof(SetSignals));
        base._Ready();
    }

    private void SetSignals()
    {
        Mdmc.Code.Game.Arena.ArenaManager.Instance.GetCurrentArena().Victory += EndAndReport;
    }

    public override void _ExitTree()
    {
		if(Instance == this )
		{
			Instance = null;
		}
        //TODO parse combat log and save it.
        base._ExitTree();
    }

    public void StartCombat()
    {
        if(_currentState == CombatState.None)
        {
            CombatStartTime = GameManager.Instance.GameClock;
            _currentState = CombatState.Running;
        }
        else if(_currentState == CombatState.Stopped)
        {
            CombatStartTime = GameManager.Instance.GameClock;
            _currentState = CombatState.Running;
        }
    }

    public void ResetCombat()
    { 
        GD.Print("Resitting Combat!");
        _currentState = CombatState.Stopped;
        Messages = new List<CombatMessage>();
        _damage = new List<EntityContributionTracker>();
        _deaths = new List<EntityContributionTracker>();
        _heal = new List<EntityContributionTracker>();
        _enmity = new List<EntityContributionTracker>();
        _effect = new List<EntityContributionTracker>();
        IsInCombat = false;
        CombatStartTime = GameManager.Instance.GameClock;
        Rpc(nameof(SyncReset));
    }

    public void EndAndReport()
    {
        GD.Print("Ending combat and reporting!");
        var players = Mdmc.Code.Game.Arena.ArenaManager.Instance.GetCurrentArena().GetPlayers();
        if (!(players.Count() > 0)) return;

        var participants = (from player in players
            let dps =
                _damage.Any(d => d.Id == player.Id)
                    ? _damage.First(d => d.Id == player.Id).TotalValue
                    : 0
            let hps =
                _heal.Any(h => h.Id == player.Id)
                    ? _heal.First(h => h.Id == player.Id).TotalValue 
                    : 0
            let death = _deaths.Any(d => d.Id == player.Id)
                    ? _deaths.First(d => d.Id == player.Id).TotalValue
                    : 0
            select MakeParticipantFromPlayer(player, dps, hps, death)).ToList();

        var record = new ServerArenaRecord()
        {
            ArenaId = Arena.ArenaManager.Instance.GetCurrentArena().Data.Id,
            Runtime = GetTimeLapsed(),
            Players = participants,
            Progress = 1,
            Time = DateTime.Now.ToUniversalTime().Ticks,
        };
        DaikonServerConnect.Instance.DaikonSetArenaRecord(record);
    }

    private ArenaParticipant MakeParticipantFromPlayer(PlayerEntity player, int dps, int hps, int deaths)
    {
        var participant = new ArenaParticipant()
        {
            Id = player.Id,
            Deaths = deaths,
            Dps = dps,
            Hps = hps,
            Name = "SomePlayer",
            Build = new ParticipantBuild()
            {
                MainId = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Main).Data.Id,
                LeftId = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Left).Data.Id,
                RightId = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Right).Data.Id,
                MainSkillIds = new []
                {
                    player.Arsenal.GetSkill(MD.ContainerSlot.Main, 0).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Main, 1).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Main, 2).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Main, 3).Data.Id
                },
                LeftSkillIds = new []
                {
                    player.Arsenal.GetSkill(MD.ContainerSlot.Left, 0).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Left, 1).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Left, 2).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Left, 3).Data.Id
                },
                RightSkillIds = new []
                {
                    player.Arsenal.GetSkill(MD.ContainerSlot.Right, 0).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Right, 1).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Right, 2).Data.Id,
                    player.Arsenal.GetSkill(MD.ContainerSlot.Right, 3).Data.Id
                },
            }
        };
        return participant;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]    
    private void SyncReset()
    {
        Messages = new List<CombatMessage>();
        _damage = new List<EntityContributionTracker>(); 
        _heal = new List<EntityContributionTracker>();
        _enmity = new List<EntityContributionTracker>();
        _effect = new List<EntityContributionTracker>();
        IsInCombat = false;
        CombatStartTime = GameManager.Instance.GameClock;
    }

    public void AddCombatMessage(CombatMessage message)
    {
        UpdateEntityTracker(message);
        Messages.Add(message);
        Rpc(nameof(CombatMessageRelay), message.Caster, message.Target, message.Effect, message.Value, (int)message.MessageType );
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void CombatMessageRelay(int caster, int target, string effect, int value, int messageType)
    {
        GD.Print(" Got Combat Message ");
        if(!IsInCombat)
        {
            CombatStartTime = GameManager.Instance.GameClock;
            IsInCombat = true;
        }
        var newMessage = new CombatMessage()
        {
            Caster = caster,
            Target = target,
            Effect = effect,
            Value = value,
            MessageType = (MD.CombatMessageType)messageType
        };
        UpdateEntityTracker(newMessage);
        Messages.Add(newMessage);
    }

    private void UpdateEntityTracker(CombatMessage message)
    {
        switch(message.MessageType)
        {
            case MD.CombatMessageType.DAMAGE:
                if(_damage.Any(dd => dd.Id == message.Caster))
                {
                    var tracker = _damage.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    _damage.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.HEAL:
                if(_heal.Any(hd => hd.Id == message.Caster))
                {
                    var tracker = _heal.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    _heal.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.ENMITY:
                if(_enmity.Any(ed => ed.Id == message.Caster))
                {
                    var tracker = _enmity.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    _enmity.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.EFFECT:
                if(_enmity.Any(ed => ed.Id == message.Caster))
                {
                    var tracker = _effect.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    _effect.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.KNOCKED_OUT:
            {
                if(_deaths.Any(ed => ed.Id == message.Caster))
                {
                    var tracker = _deaths.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    _deaths.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            }
            default:
                break;
        }
    }

    public List<EntityContributionTracker> GetTracker(MD.CombatMessageType type)
    {
        switch(type)
        {
            case MD.CombatMessageType.DAMAGE:
                return _damage;
            case MD.CombatMessageType.HEAL:
                return _heal;
            case MD.CombatMessageType.ENMITY:
                return _enmity;
            case MD.CombatMessageType.EFFECT:
                return _effect;
            case MD.CombatMessageType.KNOCKED_OUT:
                return _deaths;
            default:
                return _damage;
        }
    }

    public double GetTimeLapsed()
    {
        return GameManager.Instance.GameClock - CombatStartTime;
    }

    public float GetTotalValue(MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        return list.Count == 0 ? 0 : list.Sum(x => x.TotalValue);
    }
    public float GetTotalVPS(MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        return list.Count == 0 ? 0 : list.Sum(x => x.TotalValue);
    }
    public double GetTopVPS(MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        var sum = list.Max(t => t.TotalValue);
        var time = GameManager.Instance.GameClock - CombatStartTime;
        var dps = sum / time;
        return dps;
    }
    public float GetTopValue(MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        return list.Count == 0 ? 0 : list.Max(t => t.TotalValue);  
    }
    public double GetEntityVPS(int id, MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        var tracker = list.Find(t => t.Id == id);
        if(tracker == null)
            return 0f;
        int sum = list.Find(t => t.Id == id).TotalValue;
        var time = GameManager.Instance.GameClock - CombatStartTime;
        var dps = sum / time;
        return dps;
    }
    public float GetEntityValue(int id, MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        var tracker = list.Find(t => t.Id == id);
        return tracker == null ? 0f : tracker.TotalValue;
    }
}