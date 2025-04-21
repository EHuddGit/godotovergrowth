using Godot;
using System;

public partial class Barricade : Node2D
{
	private static bool proximity = false;

	private Signals customSignals;

	public int followerCount = 0;

	public int health = 25;

	public int ID = 0;
	private int[] spriteStages = {150,300,450,600,750}; 
	private Sprite2D barSprite;
	private Label healthTag;


	public Godot.Collections.Array<bool> followerSpots =  new Godot.Collections.Array<bool>{false,false};

	public override void _Ready()
	{
		GD.Print(this.GetPath().ToString());
		var manager = GetNode<SceneManager>("/root/game/SceneManager");
		var popup = GetNode<Panel>("popup");
		customSignals = GetNode<Signals>("/root/Signals");
		barSprite = GetNode<Sprite2D>("barricadeSprite");
		healthTag = GetNode<Label>("popup/VBoxContainer/health");

		popup.Visible = false;
		customSignals.enemyDamage += damage;
		proximity = false;
		//manager.BarricadeLocations.Add(GetNode<Sprite2D>("barricadeSprite").GlobalPosition);
		manager.addBarricade(this);
		ID = manager.getID();
		
		for(int i = 0; i < manager.getBarricades().Count; i++)
		{
			GD.Print("barricade "+ ID + " located at" + manager.getBarricades()[i].GlobalPosition.X);
		}
		//barSprite.RegionEnabled = true;
		GD.Print("barsprite region enabled" + barSprite.RegionEnabled);
		GD.Print("barsprite current region" + barSprite.RegionRect);
		barSprite.RegionRect = new Rect2(300,barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
		GD.Print("barsprite new region" + barSprite.RegionRect);
		healthTag.Text = "health: " + health;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		upgrade();
		
		
	}

	public void upgrade()
	{
		
		if(Input.IsActionJustPressed("upgrade") && proximity == true)
		{
			var upgrade = GetNode<Label>("popup/VBoxContainer/upgrade");
			var scrap = GetNode<Label>("/root/game/SceneManager/testLevel2/Player/player/Camera2D/levelUI/scrap");
			var level =  GetNode<Label>("popup/VBoxContainer/level");
			
			int totalScrap = Int32.Parse(scrap.Text.Substring(7));
			int scrapCost = Int32.Parse(upgrade.Text.Substring(10,2));
			if(totalScrap >= scrapCost )
			{
				customSignals.EmitSignal(nameof(customSignals.resourceModify),scrapCost,false);
				upgrade.Text = "Upgrade : " + (scrapCost + 10) + " scrap";
				level.Text = "Barricade level : " + (Int32.Parse(level.Text.Substring(18)) + 1);

				if(barSprite.RegionRect.Position.X < spriteStages[4])
				{
					barSprite.RegionRect = new Rect2(barSprite.RegionRect.Position.X + 150,barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
					health = ((int)barSprite.RegionRect.Position.X /150 - 1) * 25;
					healthTag.Text = "health: " + health;
				}

			}
		}
	}

	public void damage(string pathid)
	{
		if(pathid ==  GetNode<Area2D>("collisionZone").GetPath())
		{
			GD.Print("barricade damaged");
			health -= 1;
			
			if(health <= 0)
			{ //originally deleted the barricade but decided it was a bad idea since it can be rebuilt
				// instead its collisonlayer is getting removed for enemies to pass through
				GetNode<Area2D>("collisionZone").CollisionLayer = 0;
				//this.Visible = false;
				GD.Print("barricade destroyed");
				barSprite.RegionRect = new Rect2(spriteStages[0],barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
				
			}
			else if(health < 25 && health > 0)
				barSprite.RegionRect = new Rect2(spriteStages[1],barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
			else if(health < 50 && health > 25)
				barSprite.RegionRect = new Rect2(spriteStages[2],barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
			else if(health < 75 && health > 50)
				barSprite.RegionRect = new Rect2(spriteStages[3],barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
			else if(health < 100 && health > 75)
				barSprite.RegionRect = new Rect2(spriteStages[4],barSprite.RegionRect.Position.Y,barSprite.RegionRect.Size);
			
			healthTag.Text = "health: " + health;
		}
	}

	public void interaction(Node2D body)
	{
		var barricade = GetNode<Sprite2D>("barricadeSprite");
		var popup = GetNode<Panel>("popup");

		GD.Print("interaction has been called, proximity is: " + proximity);
		if(!proximity)
		{
			proximity = true;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 1);
			customSignals.EmitSignal(nameof(customSignals.objectNearby),barricade, followerSpots);
			popup.Visible = true;
			//popup.PopupOnParent(rect);
			GD.Print("popup visibility is: " + popup.Visible);
		}
		else
		{
			proximity = false;
			(barricade.Material as ShaderMaterial).SetShaderParameter("onoff", 0);
			popup.Visible = false;
			GD.Print("popup visibility is: " + popup.Visible);
		}
	}
}
