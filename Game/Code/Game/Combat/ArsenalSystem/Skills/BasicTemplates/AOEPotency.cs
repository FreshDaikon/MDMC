using Godot;

public partial class AOEPotency : Skill
{


    public override SkillResult TriggerResult()
    {
        // Implement your logic here
        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
    }

    public override SkillResult CheckSkill()
    {
        // Implement your logic here
        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST};
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public override void SkillRealization(int value, int type)
    {
        // Implement your logic here
    }
}