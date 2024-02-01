using Godot;
using System;

public partial class Utils : Node
{
	private Decal myDecal; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		myDecal = (Decal)GetNode(GetPath());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
