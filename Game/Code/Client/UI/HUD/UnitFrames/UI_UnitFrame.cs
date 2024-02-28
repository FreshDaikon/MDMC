using Godot;
using System;
using Daikon.Game;

namespace Daikon.Client;

public partial class UI_UnitFrame : Control
{
	[Export]
	private Vector2 BarSize = new Vector2(200f, 100f);
	[Export]
	private float ChangeSpeed = 5f;

	private ColorRect BarBG;
	private ColorRect SelectionIndicator;
	private TextureRect HealthBar;
	private TextureRect ShieldBar;
	private Label NameLabel;
	private Entity unit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CustomMinimumSize = BarSize;
		BarBG = GetNode<ColorRect>("%BarBG");
		SelectionIndicator = GetNode<ColorRect>("%SelectionIndicator");
		SelectionIndicator.Visible = false;
		HealthBar = GetNode<TextureRect>("%HealthBar");
		ShieldBar = GetNode<TextureRect>("%ShieldBar");
		NameLabel = GetNode<Label>("%Name");
	}

	public void SetEntity(Entity entity)
	{
		unit = entity;		
	}

	public Entity GetEntity()
	{
		return unit;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//TODO: implemen unit check here!
		if(unit != null)
		{
			NameLabel.Text = unit.EntityName;
			if(unit is PlayerEntity)
			{
				//Set Color:
				HealthBar.Modulate = new Color("#89eb75");
				var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
				if(players == null)
					return;
				var localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
				if(localPlayer != null)
				{
					if(localPlayer.CurrentFriendlyTarget == unit)
						SelectionIndicator.Visible = true;

					else
						SelectionIndicator.Visible = false;
				}
			}
			else if(unit is AdversaryEntity)
			{
				HealthBar.Modulate = new Color("#eb7575");
				var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
				if(players == null)
					return;
				var localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
				if(localPlayer != null)
				{
					if(localPlayer.CurrentTarget == unit)
						SelectionIndicator.Visible = true;
					else
						SelectionIndicator.Visible = false;
				}
			}
			//TODO : modulate color based on either team/skills etc.
			var currentHealth = (float)unit.Status.CurrentHealth / (float)unit.Status.MaxHealth;
			HealthBar.Visible = currentHealth > 0f;
			var currentShield = (float)unit.Status.CurrentShield / (float)unit.Status.MaxHealth;
			ShieldBar.Visible = currentShield > 0f;
			var shieldClip = Mathf.Clamp((currentShield + currentHealth) - 1f, 0f, 2f);
			Modulate = unit.Status.IsKnockedOut ? new Color("#ffffff32") : new Color("#ffffffff");
			BarBG.Size = BarSize;
			HealthBar.Size = new Vector2(BarSize.X * currentHealth, BarSize.Y);
			ShieldBar.Size = new Vector2(BarSize.X * currentShield, BarSize.Y); 
			ShieldBar.Position = HealthBar.Position + new Vector2(HealthBar.Size.X - (BarSize.X * shieldClip), 0f);
		}
	}
}
