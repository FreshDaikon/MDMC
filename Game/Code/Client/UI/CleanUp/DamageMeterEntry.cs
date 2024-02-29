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
	public ColorRect playerColor;
	[Export]
	public Label playerName;
	[Export]
	public Label playerDPS;
	[Export]
	public Label playerTotal;
	[Export]
	public float EntryWidth;


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
		WeightedValue = testEntity.Arsenal.GetWeightedTotal(testEntity.Arsenal.GetArsenalSkillWeights());
    }

    public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		
		if(!CombatManager.Instance.IsInCombat)
			return;		
		var testEntity = ArenaManager.Instance.GetCurrentArena().GetEntities()
			.Where(e => e is PlayerEntity)
			.Cast<PlayerEntity>()
			.ToList()
			.Find(p => p.Name == EntryId.ToString()); 
		
		if(testEntity == null)
		{
			playerName.Text = "Unknown Entity";
			playerColor.Color = new Color("#545454");
			UpdateNumbers();
		}
		else
		{
			playerName.Text = testEntity.EntityName;
			playerColor.Color = MD.GetPlayerColor(WeightedValue);
			UpdateNumbers();
			
		}
		base._Process(delta);
	}
	private void UpdateNumbers()
	{
			var vps = CombatManager.Instance.GetEntityVPS(EntryId, BarType);
			var topVps = CombatManager.Instance.GetTopVPS(BarType);
			sortValue = vps;			
			playerDPS.Text = MD.FormatDisplayNumber((float)vps);
			playerTotal.Text = MD.FormatDisplayNumber(CombatManager.Instance.GetEntityValue(EntryId, BarType));	
			if(vps > 0 && topVps > 0)
			{
				if(vps != oldDps)
				{
					oldDps = vps;
					barTween = GetTree().CreateTween();
					barTween.TweenProperty(playerColor, "size", new Vector2(EntryWidth * ((float)vps/(float)topVps) , playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
				}				
			}			
			if(vps == 0)
			{
				barTween = GetTree().CreateTween();
				barTween.TweenProperty(playerColor, "size", new Vector2(0f , playerColor.Size.Y), 0.1f).SetTrans(Tween.TransitionType.Cubic);
								
			}
	}

}
