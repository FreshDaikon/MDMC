

using Godot;
using Mdmc.Code.Game.Data;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;

[GlobalClass]
public partial class SkillSwapComponent : ContainerComponent
{
    public int SwapSlot = 0;
    public SkillData[] SwapSkillsData;
    public int ResourceGenerated = 1;


    private Skill _currentSkill;
    private int _currentIndex = 0;    
    private Skill[] _swapSkills;

    public override void ResolveComponent()
    {
        EmitSignal(SignalName.ComponentResolved);
        if(Container.GeneratesResource)
        {
            Container.IncreaseResource(ResourceGenerated);
        }
    }

    public override void SetupComponent()
    {
        _swapSkills = new Skill[SwapSkillsData.Length];

        var fake = Container.GetSkill(SwapSlot);
        fake.AssignedSlot = -1;

        for(int i = 0; i < SwapSkillsData.Length; i++)
        {
            GD.Print("Adding Skill..");
            var newSkill = DataManager.Instance.GetSkillInstance(SwapSkillsData[i].Id);
            newSkill.AssignedSlot = -1;
            newSkill.ParentContainer = Container;
            newSkill.AssignedContainerSlot = Container.AssignedSlot;
            newSkill.Name = "SwapSkill_" + Container.Skills[i].Name + "_" + i;
            newSkill.Player = Container.Player;        
            Container.AddChild(newSkill);
            newSkill.InitSkill();
            _swapSkills[i] = newSkill;
            if(i == 0)
            {
                _currentSkill = newSkill;
                _currentSkill.AssignedSlot = SwapSlot;
            }
        }
    }

    public override void UpdateComponent()
    {
        if(Container.Arsenal.LastSkillTriggered != _currentSkill && Container.Arsenal.LastSkillTriggered.TimerType == System.MD.SkillTimerType.GCD)
        {        
            _currentIndex = 0;
            _currentSkill.AssignedSlot = -1;
            _currentSkill = _swapSkills[0];
            _currentSkill.AssignedSlot = SwapSlot;
            Rpc(nameof(SyncSwapSkill), _currentIndex);
        }
        else
        {
            if(_currentIndex == _swapSkills.Length -1)
            {
                ResolveComponent();
            }
            _currentIndex = (_currentIndex + 1) % _swapSkills.Length;
            _currentSkill.AssignedSlot = -1;
            _currentSkill = _swapSkills[_currentIndex];
            _currentSkill.AssignedSlot = SwapSlot;
            Rpc(nameof(SyncSwapSkill), _currentIndex);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSwapSkill(int index)
    {
        _currentSkill.AssignedSlot = -1;
        _currentSkill = _swapSkills[index];
        _swapSkills[index].AssignedSlot = SwapSlot;
    }
}