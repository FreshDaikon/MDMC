using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Combat.ModifierSystem;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class ModIcon: Control
{

    [Export] private TextureProgressBar _progressBar;
    [Export] private TextureRect _icon;

    public ModifierHandler Modifier;

    public override void _PhysicsProcess(double delta)
    {
        if(Modifier != null)
        {
            var lapsed = GameManager.Instance.GameClock - Modifier.StartTime;
            var prog = (lapsed / Modifier.Duration) * 100;
            _icon.Texture = Modifier.Data.Icon;
            _progressBar.Value = prog;
        }
    }
}