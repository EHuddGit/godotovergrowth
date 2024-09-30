using Godot;
using System;

public partial class Crystal : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private bool proximity = false;
	private bool beingMined = false;
	private Signals customSignals;
	private int resources = 100;
	private int currentFrame = 0;
	private Rect2[] spriteRects = {new Rect2(0f,0f,128f,128f), new Rect2(128f,0f,128f,128f),new Rect2(256f,0f,128f,128f),new Rect2(384f,0f,128f,128f),new Rect2(512f,0f,128f,128f)};
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

	public void resourceMining(Sprite2D resource)
	{
		if(resource == GetNode<Sprite2D>("crystalSprite"))
		{
			resources -= 10;
			if(currentFrame != 4)
				currentFrame++;
			GD.Print("resource gathered! crystal at " + resources + " durabilty");
			GetNode<Sprite2D>("crystalSprite").RegionRect = spriteRects[currentFrame];
		}
	}
	public override void _Ready()
	{
		GetNode<Sprite2D>("crystalSprite").RegionEnabled = true;
		GetNode<Sprite2D>("crystalSprite").RegionRect = spriteRects[0];

		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.resourceMined += resourceMining;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		 	
	}
}
