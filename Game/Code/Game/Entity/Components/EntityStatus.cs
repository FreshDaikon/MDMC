using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

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

    private List<StatMod> StatMods = new List<StatMod>();
    private RandomNumberGenerator random;


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
    }

    public override void _PhysicsProcess(double delta)
    {
        //Dunno what to do here tbf.
        base._PhysicsProcess(delta);
    }

    //Bah, this is only really relevant for player entities.
    public float GetGCDModifier(float baseGCD = 2.5f)
    {    
        var modGCD = baseGCD;    
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.GCD).ToList())
        {
            // Multiplies in all directions - so 2 mods, 1.2f and 0.9 on 2.5f = 2.7f.
            modGCD *= mod.Value;
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
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.DAMAGE_RECIEVED))
        {
            weakness += mod.Value;
        }
        workingValue = (int)(workingValue * weakness);
        
        // Base mits will add up all the mods, and subtract the total from the damage.
        // For example : 2 mods, 0.4 and 0.4 = 0.8 * damage. (20% less damage taken).
        var baseMit = 0f;
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.BASE_MITS))
        {
            baseMit += mod.Value;
        }
        workingValue = (int)(workingValue - workingValue * baseMit);

        // Active Mitigation will be a multiplicative modifier. (multiply all the mods together, and subtract the total from the damage)
        // For example : 2 mods, 0.8 and 0.95 = 0.76 * damage. (24% less damage taken).
        var activeMits = 1f;
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.ACTIVE_MITS))
        {
            activeMits *= mod.Value;
        }
        workingValue = (int)(workingValue * activeMits);
        CurrentHealth -= workingValue;
        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            //TODO: knock out entity!
            EmitSignal(SignalName.KnockedOut);
            MD.Log("Entity got knocked out!");
        }
        MD.Log("I took :" + workingValue + " Damage!");
        EmitSignal(SignalName.DamageTaken, (float)workingValue);
        Rpc(nameof(UpdateHealth), CurrentHealth);
        return workingValue;
    }

    public int InflictHeal(int amount, Entity entity)
    {
        float variance = random.RandfRange(1f, 1.1f);
        amount = (int)(amount * variance);

        var healMods = 1f;
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.HEAL_RECIEVED))
        {
            healMods += mod.Value;
        }        
        int adjusted = (int)(amount * healMods);

        CurrentHealth += adjusted;
        if(CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        MD.Log("I Was Healed for :" + adjusted + " Health!");
        EmitSignal(SignalName.HealTaken, (float)adjusted);
        Rpc(nameof(UpdateHealth), CurrentHealth);
        return adjusted;
    }
   
    public float GetCurrentSpeed()
    {
        var speed = BaseSpeed;
        // Flat values expected, so just add them up.
        // For example : 2 mods, 10 and 20 = BaseSpeed + 30 = 35.
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.SPEED).ToList())
        {
            speed += mod.Value;
        }
        return speed;
    }

    public float GetDamageMultiplier()
    {
        var damage = DamageMultiplier;
        var baseDamage = 1f;
        foreach(var mod in StatMods.Where(m => m.Category == MD.ModCategory.DAMAGE_DONE).ToList())
        {
            baseDamage *= mod.Value;
        }
        MD.Log("Damage Multiplier is :" + damage + "!"); 
        return damage;
    }
    //For Setting up Stat Mods:
    public void AddStatMod(StatMod mod)
    {
        StatMods.Add(mod);
        if(mod.Category == MD.ModCategory.MAX_HEALTH)
        {
            MaxHealth += (int)mod.Value;
        }        
    }
    public void RemoveStatMod(StatMod mod)
    {
        StatMods.Remove(mod);
        if(mod.Category == MD.ModCategory.MAX_HEALTH)
        {
            MaxHealth -= (int)mod.Value;
        }
        if(CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
            Rpc(nameof(UpdateHealth), CurrentHealth);
        }
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

    private void UpdateStatMods()
    {

    }
}