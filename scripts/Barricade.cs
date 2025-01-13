using Godot;
using System;

public partial class Barricade : Node2D
{
	private bool proximity = false;

	private Signals customSignals;

	public int followerCount = 0;

	public Godot.Collections.Array<bool> followerSpots =  new Godot.Collections.Array<bool>{false,false};

	public override void _Ready()
	{
		customSignals = GetNode<Signals>("/root/Signals");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void interaction(Node2D body)
	{
		var  barricade = GetNode<Sprite2D>("barricadeSprite");
		GD.Print("interaction has been called, proximity is: " + proximity);
		if(proximity)
		{
			proximity = false;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 1);
			customSignals.EmitSignal(nameof(customSignals.objectNearby),barricade, followerSpots);
		}
		else
		{
			proximity = true;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
	}
}
