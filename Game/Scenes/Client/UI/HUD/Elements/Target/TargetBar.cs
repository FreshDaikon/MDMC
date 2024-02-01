using Godot;

public partial class TargetBar : Control
{

	[Export]
	private ColorRect targetHealthBar;
	[Export]
	private Label targetName;
	[Export]
	private Label targetHealthPercent;
	[Export]
	private bool FriendlyBar = true;
	[Export]
	private float BarSize;

	private PlayerEntity localPlayer;
	private Entity BarTarget;

	public override void _Ready()
	{
		Visible = false;
		targetHealthBar.MouseEntered += () => TryUpdateUITarget();
		targetHealthBar.MouseExited += () => UnsetUITarget();
	}

	public override void _Process(double delta)
	{	
		if(localPlayer == null)
		{			
			localPlayer = GameManager.Instance.GetPlayers().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		}
		else
		{
			var target = FriendlyBar ? localPlayer.CurrentFriendlyTarget : localPlayer.CurrentTarget;
			if(target != null)
			{
				BarTarget = target;
				Visible = true;
				targetName.Text = target.EntityName;
				var healthPercent = 100f * ((float)target.Status.CurrentHealth / (float)target.Status.MaxHealth);
				targetHealthPercent.Text = healthPercent.ToString( healthPercent < 5f ? "0.00" : "0") + "%";
				targetHealthBar.Size = new Vector2( BarSize * ((float)target.Status.CurrentHealth / (float)target.Status.MaxHealth) , targetHealthBar.Size.Y);
			}
			else
			{
				Visible = false;
			}
		}
		

	}
	// TODO #8 : Make the UITarget a better system.
	private void TryUpdateUITarget()
	{
		PlayerHUD.Instance.SetLocalPlayerUITarget(BarTarget);
	}
	private void UnsetUITarget()
	{
		PlayerHUD.Instance.SetLocalPlayerUITarget(null);
	}
}
