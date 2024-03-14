using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat;

public class SkillResult
{
    public required bool SUCCESS { get; init; }
    public required MD.ActionResult result { get; init; }
    public dynamic extraData { get; init; }
}