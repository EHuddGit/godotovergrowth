using Godot;
using System;

public partial class Crystal : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private bool proximity = false;
	private bool beingMined = false;
	private Signals customSignals;
	public Godot.Collections.Array<bool> followerSpots =  new Godot.Collections.Array<bool>{false,false};
	private int resources = 100;
	private int currentFrame = 0;
	private Rect2[] spriteRects = {new Rect2(0f,0f,96f,96f), new Rect2(96f,0f,96f,96f),new Rect2(192f,0f,96f,96f),new Rect2(288f,0f,96f,96f),new Rect2(384f,0f,96f,96f)};

	public override void _Ready()
	{
		GetNode<Sprite2D>("crystalSprite").RegionEnabled = true;
		GetNode<Sprite2D>("crystalSprite").RegionRect = spriteRects[0];
		var interactZone = GetNode<Area2D>("interactionZone");

		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.resourceMined += resourceMining;
		interactZone.BodyEntered += interaction;
		interactZone.BodyExited += interaction;
	}
	public void interaction(Node2D body)
	{
		var crystal = GetNode<Sprite2D>("crystalSprite");
		GD.Print("at garbage: this entered or exited: " + body.GetPath().ToString());
		//GD.Print("interaction has been called, proximity is: " + proximity);

		var area = GetNode<Area2D>("interactionZone");
		GD.Print("this entered or exited: " + body.GetPath().ToString());
		var bodies = area.GetOverlappingBodies();
		bool change = false;
		foreach (var bodyin in bodies)
		{
			if(bodyin == body)
			{
				change = true;
				GD.Print("player near soldier");
			}
		}

		if(change)
			proximity = true;
		else
			proximity = false;

		if(!proximity)
		{
			(crystal.Material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
		else
		{
			(crystal.Material as ShaderMaterial).SetShaderParameter("onoff", 1);
			customSignals.EmitSignal(nameof(customSignals.objectNearby),crystal, followerSpots);
		}
	}

	public void resourceMining(Sprite2D resource)
	{
		if(resource == GetNode<Sprite2D>("crystalSprite"))
		{
			resources -= 5;
			if(currentFrame != 4 && resources % 20 == 0)
				currentFrame++;
			GD.Print("resource gathered! crystal at " + resources + " durabilty");
			GetNode<Sprite2D>("crystalSprite").RegionRect = spriteRects[currentFrame];
			customSignals.EmitSignal(nameof(customSignals.resourceModify),5,true);
		}
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		 	
	}
}
