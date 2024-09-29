using Godot;
using System;

public partial class Crystal : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private bool proximity = false;
	private Signals customSignals;
	public void interaction(Node2D body)
	{
		var crystal = GetNode<Sprite2D>("crystalSprite");
		GD.Print("interaction has been called, proximity is: " + proximity);
		if(proximity)
		{
			proximity = false;
			(crystal.Material as ShaderMaterial).SetShaderParameter("onoff", 1);
			customSignals.EmitSignal(nameof(customSignals.objectNearby),crystal);
		}
		else
		{
			proximity = true;
			(crystal.Material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
	}
	public override void _Ready()
	{
		customSignals = GetNode<Signals>("/root/Signals");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		 	
	}
}
