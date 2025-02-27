using Godot;
using System;

public partial class Barricade : Node2D
{
	private static bool proximity = false;

	private Signals customSignals;

	public int followerCount = 0;

	public int health = 10;

	public int ID = 0;


	public Godot.Collections.Array<bool> followerSpots =  new Godot.Collections.Array<bool>{false,false};

	public override void _Ready()
	{
		GD.Print(this.GetPath().ToString());
		var manager = GetNode<SceneManager>("/root/game/SceneManager");
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.enemyDamage += damage;
		proximity = false;
		//manager.BarricadeLocations.Add(GetNode<Sprite2D>("barricadeSprite").GlobalPosition);
		manager.addBarricade(this);
		ID = manager.getID();
		
		for(int i = 0; i < manager.getBarricades().Count; i++)
		{
			GD.Print("barricade "+ ID + " located at" + manager.getBarricades()[i].GlobalPosition.X);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void damage(string pathid)
	{
		if(pathid ==  GetNode<Area2D>("collisionZone").GetPath())
		{
			GD.Print("barricade damaged");
			health -= 1;
			if(health == 0)
			{ //originally deleted the barricade but decided it was a bad idea since it can be rebuilt
				// instead its collisonlayer is getting removed for enemies to pass through
				GetNode<Area2D>("collisionZone").CollisionLayer = 0;
				this.Visible = false;
				GD.Print("barricade destroyed");
				//customSignals.enemyDamage -= damage;
				//this.QueueFree();
			}
		}
	}

	public void interaction(Node2D body)
	{
		var  barricade = GetNode<Sprite2D>("barricadeSprite");
		GD.Print("interaction has been called, proximity is: " + proximity);
		if(!proximity)
		{
			proximity = true;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 1);
			customSignals.EmitSignal(nameof(customSignals.objectNearby),barricade, followerSpots);
		}
		else
		{
			proximity = false;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
	}
}
