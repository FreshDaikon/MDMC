using Godot;
using SkillData = Mdmc.Code.Game.Data.SkillData;

namespace Mdmc.Code.Client.UI.Ingame.Elements;

public partial class SkillButton : Control
{
	// Exported:
	[Export] private TextureButton _button;
	
	// Internal:
	private SkillData _assignedSkill;
	
	// Signals:
	[Signal] public delegate void SkillPressedEventHandler(int id);
	
	public override void _Ready()
	{
		_button = GetNode<TextureButton>("%Button");
		_button.Pressed += () => { EmitSignal(SignalName.SkillPressed, _assignedSkill.Id ); };
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_assignedSkill != null)
		{
			_button.TextureNormal = _assignedSkill.Icon;
		}
	}

	public void AssignSkill(SkillData data)
	{
		_assignedSkill = data;
	}
}
