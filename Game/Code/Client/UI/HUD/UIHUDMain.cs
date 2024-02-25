using Godot;
using Daikon.Game;

namespace Daikon.Client;

public partial class UIHUDMain : Control
{
	// Called when the node enters the scene tree for the first time.
	public static UIHUDMain Instance;

	private PlayerEntity localPlayer;

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
		if(!GameManager.Instance.IsGameRunning())
		{
			Visible = false;
			return;
		}

		Visible = true;
		fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
		if(localPlayer == null)
		{
            var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
            if(players == null)
            {
                return;
            }
			localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
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

		}
	}
}
