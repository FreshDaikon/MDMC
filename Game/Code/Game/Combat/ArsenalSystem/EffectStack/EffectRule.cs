using Godot;

namespace Daikon.Game;


public class EffectRule
{
    public EffectData TriggerEffectData { get; init; }
    public bool IsConditional { get; init; }

    public Skill TriggerSkill { get; private set; }
    public bool PreviousOutcome { get; private set; }
    public Skill OriginSkill { get; private set; }
    public bool HasData { get; private set; } = false;
    public bool WasResolved { get; private set; } = true;

    private int _resolveTries = 0;

    public void Init(Skill owner)
    {
        OriginSkill = owner;
    }
    
    public void SetTrigger(Skill triggerSkill, bool previousOutcome)
    {
        GD.Print("Setting Trigger data..");
        TriggerSkill = triggerSkill;
        PreviousOutcome = previousOutcome;
        HasData = true;
    }
    
    public EffectData Trigger()
    {
        if (!HasData)
        {
            return new EffectData();
        }

        if (!CheckCondition()) return new EffectData();
        GD.Print("Condition was ok! try and resolve");
        TryResolve();
        return GetEffect();
    }

    public virtual void TryResolve()
    {
        if (_resolveTries >= 10)
        {
            GD.Print("We failed in 10 attempts..");
            SetWasResolved(true);
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

    public virtual EffectData GetEffect()
    {
        return TriggerEffectData;
    }
}