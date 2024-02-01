using Godot;
using System;
using System.Linq;

public partial class RoleSpread : Control
{
	[Export]
	private ColorRect DPSRect;
	[Export]
	private ColorRect TANKRect;
	[Export]
	private ColorRect HEALRect;
	[Export]
	private ColorRect RoleRECT;

	[Export]
	private float FulLRectSize;

	private float[] roleWeights;
	private float roleColorValue;
	private PlayerEntity localPlayer;

	private bool hasBeenSet = false;
	public override void _Process(double delta)
	{
		if(localPlayer == null)
		{
			localPlayer = GameManager.Instance.GetPlayers().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		}
		else if(localPlayer != null && !hasBeenSet)
		{
			hasBeenSet = true;
			roleWeights = localPlayer.Arsenal.GetArsenalSkillWeights();
			roleColorValue = localPlayer.Arsenal.GetWeightedTotal(roleWeights);
			RoleRECT.Color = MD.GetPlayerColor(roleColorValue);

			DPSRect.Color = new Color("#e34f22");	
			DPSRect.Position = new Vector2(0F, 0F);
			DPSRect.Size = new Vector2((roleWeights[0] / roleWeights.Sum()) * FulLRectSize, DPSRect.Size.Y);

			TANKRect.Color = new Color("#4164cd");	
			TANKRect.Position = new Vector2(DPSRect.Position.X + DPSRect.Size.X, 0F);
			TANKRect.Size = new Vector2((roleWeights[1] / roleWeights.Sum()) * FulLRectSize, TANKRect.Size.Y);

			HEALRect.Color = new Color("#2d9b78");	
			HEALRect.Position = new Vector2(TANKRect.Position.X + TANKRect.Size.X, 0F);
			HEALRect.Size = new Vector2((roleWeights[2] / roleWeights.Sum()) * FulLRectSize, HEALRect.Size.Y);	


		}
		
	}
}
