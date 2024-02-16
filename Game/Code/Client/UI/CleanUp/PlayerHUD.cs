using Godot;
using Daikon.Game;

namespace Daikon.Client;

public partial class PlayerHUD : Control
{
	// Called when the node enters the scene tree for the first time.
	public static PlayerHUD Instance;

	private PlayerEntity localPlayer;
	// Labels :
	[Export]
	private Label idLabel;
	[Export]
	private Label healthLabel;
	[Export]
	private Label speedLabel;
	[Export]
	private HBoxContainer modContainer;

	[Export(PropertyHint.File)]
	private string modPath;

	[Export(PropertyHint.File)]
	private string dmgNumberPath;
	private PackedScene dmgNumberResource;
	[Export]
	private Control dmgNumberContainer;

	private Label fpsLabel;
	public Camera3D activeCamera;
	public Entity LocalPlayerUITarget;

	public override void _Ready()
	{
		if(Instance != null)
		{
			QueueFree();
			return;
		}
		Instance = this;
		dmgNumberResource = (PackedScene)ResourceLoader.Load(dmgNumberPath);
		Visible = false;
		fpsLabel = GetNode<Label>("%FPS");
	}

	public void SpawnDamageNumber(string value, Color color, Vector3 worldPos)
	{		
		var inst = (DamageNumber)dmgNumberResource.Instantiate();
		dmgNumberContainer.AddChild(inst);
		inst.Initialize(value, color, worldPos);
	}
	public void SetLocalPlayerUITarget(Entity entity)
	{
		LocalPlayerUITarget = entity;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
		if(localPlayer == null)
		{
			localPlayer = ArenaManager.Instance.GetCurrentArena().GetPlayers().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
			if(localPlayer == null)
			{
				Visible = false;
			}
			else
			{
				Visible = true;
			}
		}
		else
		{
			//Basic Values:
			idLabel.Text = "Current UI Target" + (LocalPlayerUITarget == null ? "NO UI TARGET" : LocalPlayerUITarget.Name);
			healthLabel.Text = localPlayer.Status.CurrentHealth.ToString();
			speedLabel.Text = localPlayer.Status.GetCurrentSpeed().ToString();

			// Modifiers:
			// Clean up first...???
			foreach(var mod in modContainer.GetChildren())
			{
				mod.Free();
			}
			foreach(var mod in localPlayer.Modifiers.GetModifiers())
			{
				var drawMod = (PackedScene)ResourceLoader.Load(modPath);
				var instance = drawMod.Instantiate();
				var label = instance.GetNode("%TimeLeft") as Label;
				label.Text = (mod.GetTimeRemaining() * mod.Duration).ToString("0");
				modContainer.AddChild(instance);
			}
		}
	}
}
