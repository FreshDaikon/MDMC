using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Combat.Modifiers;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Entity.Components;

public partial class EntityStatus : Node
{
    //Base Values to use before mods:
    [Export] private int _baseSpeed = 5;
    //Getters for max variables:
    [Export] public int MaxHealth { get; private set; } = 10000;
    //Getters for Current Stats:
    public int CurrentHealth { get; private set; } = 0;
    public int CurrentShield { get; private set; } = 0;
    public int CurrentSpeed { get; private set; } = 0;

    
    private int _lastHealth;
    private int _lastShield;
    private int _lastSpeed;
    private StatusState _lastState;

    //References and utility:
    private RandomNumberGenerator _random;
    private Entity _entity;
    private EntityModifiers _modifiers;
    
    public enum StatusState
    {
        Alive,
        KnockedOut,
        Stunned,
    }

    public StatusState CurrentState { get; private set; } = StatusState.Alive;
    
    //Signals
    [Signal]
    public delegate void DamageTakenEventHandler(int damage, Entity entity);
    [Signal]
    public delegate void HealTakenEventHandler(int heal, Entity entity);
    [Signal]
    public delegate void KnockedOutEventHandler();

    [Signal]
    public delegate void StateChangeEventHandler(StatusState state);


    public override void _Ready()
    {
        _entity = GetParent<Entity>();
        //Init Values ?
        CurrentHealth = MaxHealth;
        CurrentSpeed = _baseSpeed;
        GD.Print("Current Speed should be " + CurrentSpeed);
        
        _lastSpeed = CurrentSpeed;
        _lastHealth = CurrentHealth;
        _lastShield = CurrentShield;
        _lastState = CurrentState;
        
        _modifiers = _entity.GetNode<EntityModifiers>("%EntityModifiers");

        if (!Multiplayer.IsServer()) return;
        _random = new RandomNumberGenerator();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Multiplayer.IsServer()) return;
        if(!GameManager.Instance.IsGameRunning()) return;
        
        CalculateSpeed();
        CalculateHealth();
        CalculateShields();

        if (_lastState != CurrentState)
        {
            _lastState = CurrentState;
            Rpc(nameof(SyncState), (int)CurrentState);
        }
        
        if(_lastSpeed != CurrentSpeed)
        {
            _lastSpeed = CurrentSpeed;
            Rpc(nameof(SyncSpeed), CurrentSpeed);
        }

        if (_lastHealth != CurrentHealth)
        {
            _lastHealth = CurrentHealth;
            Rpc(nameof(SyncHealth), CurrentHealth);
        }

        if (_lastShield != CurrentShield)
        {
            _lastShield = CurrentShield;
            Rpc(nameof(SyncShields), CurrentShield);
        }

    }

    private void CalculateSpeed()
    {
        CurrentSpeed = _baseSpeed;
        var mods = _modifiers.GetModifiers();
        if (mods != null)
        {
            var speed = mods.Where(m => m.Tags.Contains(Modifier.ModTags.MoveSpeed));
            if (speed.Any())
            {
                CurrentSpeed = _baseSpeed + (int)speed.Sum(s => s.ModifierValue);
            }
            
        }
    }

    private void CalculateHealth()
    {
        if (CurrentHealth <= 0 && CurrentState != StatusState.KnockedOut)
        {
            GD.Print("Sending Knock out signal");
            EmitSignal(SignalName.KnockedOut);
            CurrentState = StatusState.KnockedOut;
            
            if(_entity is PlayerEntity)
            {
                CombatManager.Instance.AddCombatMessage(new CombatMessage()
                {
                    Caster = int.Parse(_entity.Name),
                    Effect = "Knocked Out",
                    MessageType = MD.CombatMessageType.KNOCKED_OUT,
                    Value = 1,
                    Target = -1
                });
            }
            return;
        }
        // Figure out Regen:
        var mods = _modifiers.GetModifiers();
        if (mods != null)
        {
            var regen = mods.Where(m => m.Tags.Contains(Modifier.ModTags.Regen));
            if (regen.Any())
            {
                CurrentHealth += (int)regen.Sum(v => v.ModifierValue);
            }
        }
        
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    }

    private void CalculateShields()
    {
        var mods = _modifiers.GetModifiers();
        if (mods == null)
        {
            CurrentShield = 0;
            return;
        }
        var shields = mods.Where(m => m.Tags.Contains(Modifier.ModTags.Shield));
        if (!shields.Any())
            CurrentShield = 0;
        CurrentShield = (int)shields.Sum(s => s.RemainingValue);
    }

    public int InflictDamage(int amount, Entity entity)
    {
        if (CurrentState == StatusState.KnockedOut)
            return 0;
        
        // A little bit of boiler plate:
        var workingValue = amount;
        var variance = _random.RandfRange(1.0f, 1.05f);
        //Apply a bit of variance:
        workingValue = (int)(workingValue * variance);
        // Time to get all the mods:
        var mods = _modifiers.GetModifiers();

        if (mods != null)
        {
            // First Increase the value by damage taken: (Note : we might want to move mits before weakness to make it more scary.)
            var weaknesses = mods.Where(m => m.Tags.Contains(Modifier.ModTags.DamageTaken));
            if(weaknesses.Any())
            {
                // Expected values : 0.1 + 0.4 + 0.4 = 1 + 0.9 = 1.9 = value * 1.9 for example.
                var multiplier = weaknesses.Sum(w => w.ModifierValue);
                workingValue = (int)(workingValue * (1 + multiplier));
            }
            // Now that the value is greater - we can reduce it by mitigation:
            var mits = mods.Where(m => m.Tags.Contains(Modifier.ModTags.Mitigation));
            if (mits.Any())
            {
                // Expected values : 0.8 * 0.8 * 0.9 = 0.576 = value * 0.576 for example.
                var factor = mits.Aggregate(1d, (current, mit) => current * mit.ModifierValue);
                workingValue = (int)(workingValue * Mathf.Clamp(factor, 0.4d, 1d)); // Maximum is 60%
            }
            // Now then we can reduce the remaining damage by shields.
            var shields = mods.Where(m => m.Tags.Contains(Modifier.ModTags.Shield));
            if (shields.Any())
            {
                var remainder = workingValue;
                foreach (var shield in shields)
                {
                    var stored = remainder;
                    remainder = Mathf.Clamp(remainder - (int)shield.RemainingValue, 0, remainder);
              
                    var newValue = shield.RemainingValue - stored;
                    if (newValue < 0)
                    {
                        _modifiers.RemoveModifier(shield.Data.Id);
                    }
                    else
                    {
                        shield.RemainingValue = newValue;
                    }
                    // Check if damage was fully absorbed:
                    if (remainder == 0)
                        break;
                }
                workingValue = remainder;
            }
        }

        // Now that we have modified the incoming damage in all kinds of interesting ways, simply subtract it:
        CurrentHealth -= workingValue;
        
        // Check for knocked out state:
        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            return 0;            
        }
        EmitSignal(SignalName.DamageTaken, workingValue);
        return workingValue;
    }
    
    public int InflictHeal(int amount, Entity entity)
    {
        var variance = _random.RandfRange(1f, 1.1f);
        amount = (int)(amount * variance);

        var healMods = 1f;
        var mods = _modifiers.GetModifiers();
        if(mods != null)
        {
            healMods += (float)mods.Where(m => 
                m.Tags.Contains(Modifier.ModTags.Buff) && m.Tags.Contains(Modifier.ModTags.HealPower))
                .ToList()
                .Sum(x => x.ModifierValue);
        }        
        int adjusted = (int)(amount * healMods);

        CurrentHealth += adjusted;
        if(CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        GD.Print("I Was Healed for :" + adjusted + " Health!");
        EmitSignal(SignalName.HealTaken, (float)adjusted);
        return adjusted;
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
        CurrentState = StatusState.Alive;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncHealth(int newHealth)
    {
        CurrentHealth = newHealth;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncShields(int newShield)
    {
        CurrentShield = newShield;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSpeed(int speed)
    {
        CurrentSpeed = speed;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncState(int state)
    {
        CurrentState = (StatusState)state;
    }
    
}