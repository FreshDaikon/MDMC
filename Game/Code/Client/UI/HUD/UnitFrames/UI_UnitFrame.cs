using Godot;
using System;
using Daikon.Game;
using Daikon.Helpers;

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

	[Export] private Vector2 CastSize = new Vector2(188f, 18f);
	[Export] private ColorRect CastBG;
	[Export] private TextureRect CastBar;
	

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
		CastBG.Visible = false;
		//TODO: implemen unit check here!
		if(unit != null)
		{
			NameLabel.Text = unit.EntityName;
			if(unit is PlayerEntity)
			{
				var player = unit as PlayerEntity;
				//Set Color:
				var WeightedValue = player.Arsenal.GetWeightedTotal(player.Arsenal.GetArsenalSkillWeights());
				HealthBar.Modulate = MD.GetPlayerColor(WeightedValue);
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
				var adv = (AdversaryEntity)unit;
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

				if (adv._isCasting)
				{
					CastBG.Visible = true;
					var lapsed = GameManager.Instance.GameClock - adv._startTime;
					CastBar.Size = new Vector2(CastSize.X * (float)(lapsed / adv._castTime), CastSize.Y);
				}
			}
			//TODO : modulate color based on either team/skills etc.
			var currentHealth = (float)unit.Status.CurrentHealth / (float)unit.Status.MaxHealth;
			HealthBar.Visible = currentHealth > 0f;
			var currentShield = (float)unit.Status.CurrentShield / (float)unit.Status.MaxHealth;
			ShieldBar.Visible = currentShield > 0f && unit.Status.CurrentState != EntityStatus.StatusState.KnockedOut;;
			var shieldClip = Mathf.Clamp((currentShield + currentHealth) - 1f, 0f, 2f);
			BarBG.Size = BarSize;
			HealthBar.Size = new Vector2(BarSize.X * currentHealth, BarSize.Y);
			ShieldBar.Size = new Vector2(BarSize.X * currentShield, BarSize.Y); 
			ShieldBar.Position = HealthBar.Position + new Vector2(HealthBar.Size.X - (BarSize.X * shieldClip), 0f);
		}
	}
}
