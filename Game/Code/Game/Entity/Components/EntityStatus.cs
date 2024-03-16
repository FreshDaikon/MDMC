using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Combat.ModifierSystem;
using Mdmc.Code.Game.Combat.ModifierSystem.Buffs;
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
            // TODO : imple ment speed buffs.            
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
                    Caster = _entity.Id,
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
            // TODO : implement the regen buff yes.
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
        var buffs = mods.SelectMany(m => m.BuffControl.Buffs).ToList();
        var shields = buffs.Any() ? buffs.Where(b => b is BuffShield).Cast<BuffShield>() : null;

        if (!shields.Any())
            CurrentShield = 0;
        CurrentShield = shields.Sum(s => s.ShieldValue);
    }

    public int InflictDamage(int amount, Entity entity)
    {
        if (CurrentState == StatusState.KnockedOut)
            return 0;
        
        // A little bit of boiler plate:
        var workingValue = amount;
        var variance = _random.RandfRange(1.0f, 1.05f);
        workingValue = (int)(workingValue * variance);
        var mods = _modifiers.GetModifiers();

        if (mods != null)
        {
            // Now then we can reduce the remaining damage by shields.
            var buffs = mods.SelectMany(m => m.BuffControl.Buffs).ToList();
            var shields = buffs.Any() ? buffs.Where(b => b is BuffShield).Cast<BuffShield>() : null;
            if (shields.Any())
            {
                GD.Print("There are some shields - reduce them by damage.");
                var remainder = workingValue;
                foreach (var shield in shields)
                {
                    remainder = shield.ImpactShield(remainder);
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