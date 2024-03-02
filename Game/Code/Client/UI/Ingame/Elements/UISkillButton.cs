using Godot;
using System;
using Daikon.Game;

public partial class UISkillButton : Control
{
	public SkillData AssignedSkill;

	private TextureButton _button;
	
	
	[Signal]
	public delegate void SkillPressedEventHandler(int id);
	
	public override void _Ready()
	{
		_button = GetNode<TextureButton>("%Button");
		_button.Pressed += () => { EmitSignal(SignalName.SkillPressed, AssignedSkill.Id ); };
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (AssignedSkill != null)
		{
			_button.TextureNormal = AssignedSkill.Icon;
		}
	}
}
