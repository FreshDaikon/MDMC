using Godot;
using SkillContainerData = Mdmc.Code.Game.Data.SkillContainerData;

namespace Mdmc.Code.Client.UI.Ingame.Elements;

public partial class ContainerButton : Control
{
	// Export :
	[Export] private TextureButton _button;
	
	// Internal :
	private SkillContainerData _assignedContainer;
	
	// Signals :
	[Signal] public delegate void ContainerPressedEventHandler(int id);
	
	public override void _Ready()
	{
		_button.Pressed += () => { EmitSignal(SignalName.ContainerPressed, _assignedContainer.Id ); };
	}
	
	public override void _Process(double delta)
	{
		if (_assignedContainer != null)
		{
			_button.TextureNormal = _assignedContainer.Icon;
		}
	}

	public void SetContainer(SkillContainerData containerData)
	{
		_assignedContainer = containerData;
	}
	
}
