using Godot;
using Mdmc.Code.Game.Data.Decorators;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;


public class Rule
{
    public EffectData TriggerEffectData { get; init; }

    public int Charges { get; init; } = 1;

    public Skill TriggerSkill { get; private set; }
    public Skill OriginSkill { get; private set; }

    public bool HasData { get; private set; } = false;
    public bool WasResolved { get; private set; } = true;

    private int _resolveTries = 0;
    private int _chargesLeft;

    public void Init(Skill owner)
    {
        OriginSkill = owner;
        _chargesLeft = Charges;
    }
    
    public void ArmTrigger(Skill triggerSkill)
    {
        TriggerSkill = triggerSkill;
        HasData = true;
    }
    
    public bool Trigger()
    {
        if (!HasData)
        {
            return false;
        }
        if (!CheckCondition()) return false;
        TryResolve();
        return true;
    }

    public virtual void TryResolve()
    {
        if (_resolveTries >= 10)
        {
            GD.Print("We failed in 10 attempts..");
            SetWasResolved(true);
        }
        if(CheckCondition())
        {
            _chargesLeft -= 1;
            if(_chargesLeft <= 0)
            {
                SetWasResolved(true);
            }
        }
        _resolveTries++;
    }

    public void SetWasResolved(bool result)
    {
        WasResolved = result;
    }

    public virtual bool CheckCondition()
    {
        return false;
    }

    public Effect GetEffect()
    {
        var effect = TriggerEffectData.GetEffect();
        return effect;
    }
}