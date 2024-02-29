using Godot;

namespace Daikon.Game;


public class EffectRule
{
    
    public Effect Effect { get; init; }
    public bool IsConditional { get; init; }

    public Skill TrigggerSkill { get; private set; }
    public bool PreviousOutcome { get; private set; }
    public Skill OriginSkill { get; private set; }
    public bool HasData { get; private set; } = false;
    public bool WasResolved { get; private set; } = true;


    public void Init(Skill owner)
    {
        OriginSkill = owner;
    }
    
    public void SetTrigger(Skill triggerSkill, bool previousOutcome)
    {
        GD.Print("Setting Trigger data..");
        TrigggerSkill = triggerSkill;
        PreviousOutcome = previousOutcome;
        HasData = true;
    }
    
    public Effect Trigger()
    {
        if (!HasData)
        {
            return new Effect();
        }

        if (!CheckCondition()) return new Effect();
        
        GD.Print("Condition was ok! try and resolve");
        TryResolve();
        return Effect;
    }

    public virtual void TryResolve() {}

    public void SetWasResolved(bool result)
    {
        WasResolved = result;
    }

    public virtual bool CheckCondition()
    {
        return false; 
    }
}