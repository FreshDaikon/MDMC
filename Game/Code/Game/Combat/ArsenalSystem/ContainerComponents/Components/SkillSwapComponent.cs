

using Godot;
using Mdmc.Code.Game.Data;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;

[GlobalClass]
public partial class SkillSwapComponent : ContainerComponent
{
    public int SwapSlot = 0;
    public SkillData[] SwapSkillsData;
    public int ResourceGenerated = 1;

    private SkillSystem.SkillHandler _currentSkill;
    private int _currentIndex = 0;    
    private SkillSystem.SkillHandler[] _swapSkills;

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
        _swapSkills = new SkillSystem.SkillHandler[SwapSkillsData.Length];

        var fake = Container.GetSkill(SwapSlot);
        fake.AssignSlot(-1);

        for(int i = 0; i < SwapSkillsData.Length; i++)
        {
            var swapSkill = SwapSkillsData[i].GetSkill();
            swapSkill.AssignSlot(-1);
            swapSkill.SetArsenal(Container.Arsenal);
            swapSkill.Name = "SwapSkill_" + Container.Skills[i].Name + "_" + i;
            Container.AddChild(swapSkill);
            _swapSkills[i] = swapSkill;
            if(i == 0)
            {
                _currentSkill = swapSkill;
                _currentSkill.AssignSlot(SwapSlot);
            }
        }
    }

    public override void UpdateComponent()
    {
        if(Container.Arsenal.LastSkillTriggered != _currentSkill && Container.Arsenal.LastSkillTriggered.GetTypeInfo() == SkillSystem.SkillHandler.SkillType.GCD)
        {        
            _currentIndex = 0;
            _currentSkill.AssignSlot(-1);
            _currentSkill = _swapSkills[0];
            _currentSkill.AssignSlot(SwapSlot);
            Rpc(nameof(SyncSwapSkill), _currentIndex);
        }
        else
        {
            if(_currentIndex == _swapSkills.Length -1)
            {
                ResolveComponent();
            }
            _currentIndex = (_currentIndex + 1) % _swapSkills.Length;
            _currentSkill.AssignSlot(-1);
            _currentSkill = _swapSkills[_currentIndex];
            _currentSkill.AssignSlot(SwapSlot);
            Rpc(nameof(SyncSwapSkill), _currentIndex);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSwapSkill(int index)
    {
        _currentSkill.AssignSlot(-1);
        _currentSkill = _swapSkills[index];
        _swapSkills[index].AssignSlot(SwapSlot);
    }
}