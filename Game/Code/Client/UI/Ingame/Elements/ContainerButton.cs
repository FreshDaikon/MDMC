using Godot;
using System;
using Daikon.Game;

namespace Mdmc.Code.Client.UI.Ingame.Elements;

public partial class UIContainerButton : Control
{
	public SkillContainerData AssignedContainer;

	private TextureButton _button;
	
	[Signal]
	public delegate void ContainerPressedEventHandler(int id);
	
	public override void _Ready()
	{
		_button = GetNode<TextureButton>("%Button");
		_button.Pressed += () => { EmitSignal(SignalName.ContainerPressed, AssignedContainer.Id ); };
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (AssignedContainer != null)
		{
			_button.TextureNormal = AssignedContainer.Icon;
		}
	}
}
