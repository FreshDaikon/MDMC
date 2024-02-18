using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class CombatManager: Node
{
    public static CombatManager Instance;   
    public List<CombatMessage> Messages = new List<CombatMessage>();
    public bool IsInCombat = false;

    //Combat Session Tracker:
    private ulong CombatStartTime = 0;


    private List<EntityContributionTracker> damage = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> heal = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> enmity = new List<EntityContributionTracker>();
    private List<EntityContributionTracker> effect = new List<EntityContributionTracker>();

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        base._Ready();
    }

    public void StartCombat()
    {
        CombatStartTime = GameManager.Instance.ServerTick;
    }

    public void AddCombatMessage(CombatMessage message)
    {
        if(!IsInCombat)
        {
            CombatStartTime = GameManager.Instance.ServerTick;
            IsInCombat = true;
        }
        UpdateEntityTracker(message);
        Messages.Add(message);
        Rpc(nameof(CombatMessageRelay), message.Caster, message.Target, message.Effect, message.Value, (int)message.MessageType );
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void CombatMessageRelay(int caster, int target, string effect, int value, int messageType)
    {
        MD.Log(" Got Combat Message ");
        if(!IsInCombat)
        {
            CombatStartTime = GameManager.Instance.ServerTick;
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
        var dmgNumberPos = ArenaManager.Instance.GetCurrentArena().GetEntity(target);
        UpdateEntityTracker(newMessage);
        Messages.Add(newMessage);
    }

    private void UpdateEntityTracker(CombatMessage message)
    {
        switch(message.MessageType)
        {
            case MD.CombatMessageType.DAMAGE:
                if(damage.Any(dd => dd.Id == message.Caster))
                {
                    var tracker = damage.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    damage.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.HEAL:
                if(heal.Any(hd => hd.Id == message.Caster))
                {
                    var tracker = heal.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    heal.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.ENMITY:
                if(enmity.Any(ed => ed.Id == message.Caster))
                {
                    var tracker = enmity.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    enmity.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            case MD.CombatMessageType.EFFECT:
                if(enmity.Any(ed => ed.Id == message.Caster))
                {
                    var tracker = effect.Find(t => t.Id == message.Caster);
                    tracker.TotalValue += message.Value;
                }
                else
                {
                    effect.Add(new EntityContributionTracker(){ Id = message.Caster, TotalValue=message.Value} );
                }
                break;
            default:
                break;
        }
    }

    public List<EntityContributionTracker> GetTracker(MD.CombatMessageType type)
    {
        switch(type)
        {
            case MD.CombatMessageType.DAMAGE:
                return damage;
            case MD.CombatMessageType.HEAL:
                return heal;
            case MD.CombatMessageType.ENMITY:
                return enmity;
            case MD.CombatMessageType.EFFECT:
                return effect;
            default:
                return damage;
        }
    }

    public float GetTimeLapsed()
    {
        return (GameManager.Instance.ServerTick - CombatStartTime) / 1000f;
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
    public float GetTopVPS(MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        var sum = list.Max(t => t.TotalValue);
        var time = (GameManager.Instance.ServerTick - CombatStartTime) / 1000f;
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
    public float GetEntityVPS(int id, MD.CombatMessageType type)
    {
        var list = GetTracker(type);
        if(list.Count == 0)
            return 0f;
        var tracker = list.Find(t => t.Id == id);
        if(tracker == null)
            return 0f;
        int sum = list.Find(t => t.Id == id).TotalValue;
        var time = (GameManager.Instance.ServerTick - CombatStartTime) / 1000f;
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