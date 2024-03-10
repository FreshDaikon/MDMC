using System.Diagnostics;
using Godot;
using System.Linq;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class DamageMeterEntry : ColorRect
{
	public int EntryId;
	public MD.CombatMessageType BarType;

	[Export]
	private ColorRect _playerColor;

	[Export] private TextureRect _edgeGlow;

	private GradientTexture1D _barGradient; 
	[Export]
	private Label _playerName;
	[Export]
	private Label _playerDps;
	[Export]
	private Label _playerTotal;
	[Export]
	public float EntryWidth;

	[Export] private TextureRect _tagLeft;
	[Export] private TextureRect _tagMain;
	[Export] private TextureRect _tagRight;


	private double oldDps;
	private Tween barTween;
	public double sortValue = 0f;

	private float[] WeightedValue;

    public override void _Ready()
    {
       var testEntity = ArenaManager.Instance.GetCurrentArena().GetEntities()
			.Where(e => e is PlayerEntity)
			.Cast<PlayerEntity>()
			.ToList()
			.Find(p => p.Name == EntryId.ToString());		
    }

    public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		
		if(!CombatManager.Instance.IsInCombat)
			return;		
		
		
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
			sortValue = vps;			
			_playerDps.Text = MD.FormatDisplayNumber((float)vps);
			_playerTotal.Text = MD.FormatDisplayNumber(CombatManager.Instance.GetEntityValue(EntryId, BarType));	
			if(vps > 0 && topVps > 0)
			{
				if(vps != oldDps)
				{
					oldDps = vps;
					barTween = GetTree().CreateTween();
					barTween.TweenProperty(_playerColor, "size", new Vector2(EntryWidth * ((float)vps/(float)topVps) , _playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
				}				
			}			
			if(vps == 0)
			{
				barTween = GetTree().CreateTween();
				barTween.TweenProperty(_playerColor, "size", new Vector2(0f , _playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
								
			}
			_edgeGlow.Position = new Vector2(_playerColor.Position.X + _playerColor.Size.X - 40, _edgeGlow.Position.Y);
	}

}
