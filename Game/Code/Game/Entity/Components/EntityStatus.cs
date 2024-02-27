using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;
using System.Runtime.Intrinsics.Arm;

namespace Daikon.Game;

public partial class EntityStatus : Node
{
    //----------------------------------------------------------
    #region SPEED
    [Export]
    public float BaseSpeed = 5f;
    #endregion
    //----------------------------------------------------------
    #region HEALTH
    public int CurrentHealth = 10000;
    [Export]
    public int MaxHealth = 10000;
    #endregion
    #region SHIELD
    public int CurrentShield = 10;
    #endregion
    //----------------------------------------------------------
    #region MITIGATION
    [Export]
    public float BaseMitigation = 0f;
    [Export]
    public float CurrentMitigation = 0f;
    #endregion
    //----------------------------------------------------------
    #region  DAMAGE
    [Export]
    public float DamageMultiplier = 1.0f;
    #endregion

    private RandomNumberGenerator random;
    private Entity entity;
    private EntityModifiers modifiers;

    //Signals
    [Signal]
    public delegate void DamageTakenEventHandler(float damage, Entity entity);
    [Signal]
    public delegate void HealTakenEventHandler(float heal, Entity entity);
    [Signal]
    public delegate void KnockedOutEventHandler();


    public override void _Ready()
    {
        if(Multiplayer.IsServer())
        {
            random = new RandomNumberGenerator();
            CurrentHealth = MaxHealth;
        }
        //Setup base values.
        entity = GetParent<Entity>();
        modifiers = entity.GetNode<EntityModifiers>("%EntityModifiers");
    }

    public override void _PhysicsProcess(double delta)
    {
        //Dunno what to do here tbf.
        base._PhysicsProcess(delta);
    }

    //Bah, this is only really relevant for player entities.
    public float GetGCDModifier(float baseGCD = 1.5f)
    {    
        var modGCD = baseGCD;    
        var mods = modifiers.GetModifiers();
        if(mods != null)
        {
            //modGCD *= (float)mods.Where(m => m.Types.Contains(Modifier.ModTypes.GCDSpeed)).ToList().Sum(x => x.ModifierValue);
        }
        return modGCD;
    }
    
    public int InflictDamage(int amount, Entity entity)
    {
        var workingValue = amount;
        float variance = random.RandfRange(1.0f, 1.05f);
        workingValue = (int)(workingValue * variance);

        var weakness = 1f;
        // Damage Recieved will be a additive weakness modifier. (add up all the mods, and multiply the total by the damage)
        // For example : 2 mods, 1.2 and 1.4 = 1.6 * damage. (60% more damage taken)
        var mods = modifiers.GetModifiers();
        if(mods != null)
        {
            weakness += (float)mods.Where(m => 
                m.Tags.Contains(Modifier.ModTags.Debuff) && m.Tags.Contains(Modifier.ModTags.DamageTaken))
                .ToList()
                .Sum(x => x.ModifierValue);
        }
        workingValue = (int)(workingValue * weakness);
        
        // Base mits will add up all the mods, and subtract the total from the damage.
        // For example : 2 mods, 0.4 and 0.4 = 0.8 * damage. (20% less damage taken).
        var baseMit = 0f;
        if(mods != null)
        {
        }
        workingValue = (int)(workingValue - workingValue * baseMit);

        // Active Mitigation will be a multiplicative modifier. (multiply all the mods together, and subtract the total from the damage)
        // For example : 2 mods, 0.8 and 0.95 = 0.76 * damage. (24% less damage taken).
        var activeMits = 1f;
        if(mods != null)
        { 
            /**
            baseMit += (float)mods.Where(m => 
                m.Types.Contains(Modifier.ModTypes.Buff) && m.Types.Contains(Modifier.ModTypes.DamageTaken))
                .ToList()
                .Select(v => v.ModifierValue)
                .ToList()
                .Aggregate((x, y) => x * y); **/
        }
        workingValue = (int)(workingValue * activeMits);
        if(CurrentShield > 0)
        {
            var stored = workingValue;
            workingValue =  Mathf.Clamp( workingValue - CurrentShield, 0, workingValue);
            GD.Print("New working Value is " + workingValue);
            CurrentShield = Mathf.Clamp(CurrentShield - stored, 0, CurrentShield);
            Rpc(nameof(UpdateShields), CurrentShield);
        }
        CurrentHealth -= workingValue;
        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            //TODO: knock out entity!
            EmitSignal(SignalName.KnockedOut);
            GD.Print("Entity got knocked out!");
        }
        GD.Print("I took :" + workingValue + " Damage!");
        EmitSignal(SignalName.DamageTaken, (float)workingValue);
        Rpc(nameof(UpdateHealth), CurrentHealth);
        return workingValue;
    }

    public int InflictHeal(int amount, Entity entity)
    {
        float variance = random.RandfRange(1f, 1.1f);
        amount = (int)(amount * variance);

        var healMods = 1f;
        var mods = modifiers.GetModifiers();
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
        Rpc(nameof(UpdateHealth), CurrentHealth);
        return adjusted;
    }

    public int InflictShield(int amount, Entity entity)
    {
        CurrentShield += amount;
        Rpc(nameof(UpdateShields), CurrentShield);
        return CurrentShield;
    }
   
    public float GetCurrentSpeed()
    {
        var speed = BaseSpeed;
        // Flat values expected, so just add them up.
        // For example : 2 mods, 10 and 20 = BaseSpeed + 30 = 35.
        // Can be negative - so get both buffs and debuffs!
        var mods = modifiers.GetModifiers();
        if(mods != null)
        {
            speed += (float)mods.Where(m => 
                m.Tags.Contains(Modifier.ModTags.MoveSpeed))
                .ToList()
                .Sum(x => x.ModifierValue);
        }
        return speed;
    }

    public float GetDamageMultiplier()
    {
        var damage = DamageMultiplier;
       // var baseDamage = 1f;
        var mods = modifiers.GetModifiers();
        if(mods != null)
        {

        }
        GD.Print("Damage Multiplier is :" + damage + "!"); 
        return damage;
    }
    public void Reset()
    {
        CurrentHealth = MaxHealth;
        Rpc(nameof(UpdateHealth), CurrentHealth);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateHealth(int newHealth)
    {
        CurrentHealth = newHealth;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateShields(int newShield)
    {
        CurrentShield = newShield;
    }
}