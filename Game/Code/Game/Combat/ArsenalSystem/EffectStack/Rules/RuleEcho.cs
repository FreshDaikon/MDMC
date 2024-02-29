using Daikon.Helpers;
using Godot;

namespace Daikon.Game;

public class RuleEcho: EffectRule
{
    //Stuff..
    public MD.SkillType TypeToEcho { get; set; }

    private int tries = 0;
    
    public override void TryResolve()
    {
        GD.Print("We MF fucking did it!");
        if(OriginSkill.TriggerSkill().SUCCESS)
        {
            GD.Print("We successfully echoed!");
            SetWasResolved(true);
        }
        else
        {
            if (tries >= 10)
            {
                GD.Print("We failed in 10 attempts..");
                SetWasResolved(true);
            }
            tries++;
        }     
    }
    public override bool CheckCondition()
    {
        GD.Print("Trigger skill is :" + (TrigggerSkill == null));
        if (TrigggerSkill != null)
        {
            GD.Print("Echo Type is :" + TypeToEcho.ToString());
            GD.Print("Trigger Type is :" + TrigggerSkill.SkillType.ToString());
            return TrigggerSkill.SkillType == TypeToEcho;
        }
        return false;
    }
}