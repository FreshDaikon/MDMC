using System.Linq;
using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Client.UI.HUD.DamageMeter;

public partial class DamageMeterEntry : ColorRect
{
	// Set by External:
	public double SortValue = 0f;
	public int EntryId;
	public MD.CombatMessageType BarType;

	//Exported:
	[Export] private ColorRect _playerColor;
	[Export] private TextureRect _edgeGlow;
	[Export] private Label _playerName;
	[Export] private Label _playerDps;
	[Export] private Label _playerTotal;
	[Export] private float _entryWidth;
	[Export] private TextureRect _tagLeft;
	[Export] private TextureRect _tagMain;
	[Export] private TextureRect _tagRight;

	//Internal:
	private GradientTexture1D _barGradient; 
	private double _oldDps;
	private Tween _barTween;
	private float[] _weightedValue;
	

    public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning()) return;
		if(!CombatManager.Instance.IsInCombat) return;
		
		
		var playerEntity = ArenaManager.Instance.GetCurrentArena().GetEntities()
			.Where(e => e is PlayerEntity)
			.Cast<PlayerEntity>()
			.ToList()
			.Find(p => p.Name == EntryId.ToString()); 
		
		if(playerEntity == null)
		{
			_playerName.Text = "Unknown Entity";
			UpdateNumbers();
		}
		else
		{
			_playerName.Text = playerEntity.EntityName;
			UpdateNumbers();
			_tagLeft.Modulate = playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Left) == null ? new Color("#ffffff") : playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Left).Data.ContainerColor;
			_tagMain.Modulate = playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Main) == null ? new Color("#ffffff") : playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Main).Data.ContainerColor;
			_tagRight.Modulate = playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Right) == null ? new Color("#ffffff") : playerEntity.Arsenal.GetSkillContainer(MD.ContainerSlot.Right).Data.ContainerColor;
			_playerColor.Modulate = BarType switch
			{
				MD.CombatMessageType.HEAL => new Color("#86bd73"),
				MD.CombatMessageType.DAMAGE => new Color("#ba4545"),
				MD.CombatMessageType.ENMITY => new Color("#5990bd"),
				MD.CombatMessageType.KNOCKED_OUT => new Color("#85417a"),
				_ => new Color("#000000")
				
			};
		}
		base._Process(delta);
	}
	private void UpdateNumbers()
	{
			var vps = CombatManager.Instance.GetEntityVPS(EntryId, BarType);
			var topVps = CombatManager.Instance.GetTopVPS(BarType);
			SortValue = vps;			
			_playerDps.Text = MD.FormatDisplayNumber((float)vps);
			_playerTotal.Text = MD.FormatDisplayNumber(CombatManager.Instance.GetEntityValue(EntryId, BarType));	
			
			if(vps > 0 && topVps > 0)
			{
				if(vps != _oldDps)
				{
					_oldDps = vps;
					_barTween = GetTree().CreateTween();
					_barTween.TweenProperty(_playerColor, "size", new Vector2(_entryWidth * ((float)vps/(float)topVps) , _playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
				}				
			}			
			if(vps == 0)
			{
				_barTween = GetTree().CreateTween();
				_barTween.TweenProperty(_playerColor, "size", new Vector2(0f , _playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
								
			}
			_edgeGlow.Position = new Vector2(_playerColor.Position.X + _playerColor.Size.X - 40, _edgeGlow.Position.Y);
	}

}
