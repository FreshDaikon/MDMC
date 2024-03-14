using System.Linq;
using Godot;
using Mdmc.Code.System;
using AdversaryEntity = Mdmc.Code.Game.Entity.Adversary.AdversaryEntity;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using Entity = Mdmc.Code.Game.Entity.Entity;
using EntityStatus = Mdmc.Code.Game.Entity.Components.EntityStatus;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class UnitFrame : Control
{
	[Export]
	private Vector2 BarSize = new Vector2(200f, 100f);
	[Export]
	private float ChangeSpeed = 5f;

	[Export] private TextureRect _edgeGlow;
	private ColorRect BarBG;
	
	private ColorRect SelectionIndicator;
	private TextureRect HealthBar;
	private TextureRect ShieldBar;
	private Label NameLabel;
	private Entity unit;

	[Export] private Vector2 CastSize = new Vector2(188f, 18f);
	[Export] private ColorRect CastBG;
	[Export] private TextureRect CastBar;

	[Export] private TextureRect _tagLeft;
	[Export] private TextureRect _tagMain;
	[Export] private TextureRect _tagRight;

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
		_edgeGlow.Visible = false;
		//TODO: implemen unit check here!
		if(unit != null)
		{
			NameLabel.Text = unit.EntityName;
			if(unit is PlayerEntity player)
			{
				//Set Color:
				_tagLeft.Modulate = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Left) == null ? new Color("#ffffff") : player.Arsenal.GetSkillContainer(MD.ContainerSlot.Left).Data.ContainerColor;
				_tagMain.Modulate = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Main) == null ? new Color("#ffffff") : player.Arsenal.GetSkillContainer(MD.ContainerSlot.Main).Data.ContainerColor;
				_tagRight.Modulate = player.Arsenal.GetSkillContainer(MD.ContainerSlot.Right) == null ? new Color("#ffffff") : player.Arsenal.GetSkillContainer(MD.ContainerSlot.Right).Data.ContainerColor;
				HealthBar.Modulate = new Color("#b1cf84");
				
				var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
				if(players == null)
					return;
				var localPlayer = players.ToList().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
				if(localPlayer != null)
				{
					if(localPlayer.CurrentFriendlyTarget == player)
						SelectionIndicator.Visible = true;

					else
						SelectionIndicator.Visible = false;
				}
			}
			else if(unit is AdversaryEntity)
			{
				_tagLeft.Visible = false;
				_tagMain.Visible = false;
				_tagRight.Visible = false;
				var adv = (AdversaryEntity)unit;
				HealthBar.Modulate = new Color("#d65858");
				var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
				if(players == null)
					return;
				var localPlayer = players.ToList().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
				if(localPlayer != null)
				{
					if(localPlayer.CurrentTarget == unit)
						SelectionIndicator.Visible = true;
					else
						SelectionIndicator.Visible = false;
				}

				if (adv._isCasting && adv.Status.CurrentState != EntityStatus.StatusState.KnockedOut)
				{
					CastBG.Visible = true;
					_edgeGlow.Visible = true;
					var lapsed = GameManager.Instance.GameClock - adv._startTime;
					CastBar.Size = new Vector2(CastSize.X * (float)(lapsed / adv._castTime), CastSize.Y);
					_edgeGlow.Position = new Vector2(CastBar.Position.X + CastBar.Size.X - 40, _edgeGlow.Position.Y );
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
