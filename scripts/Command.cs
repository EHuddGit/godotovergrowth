using Godot;
using System;



public partial class Command : Node2D
{
	private Signals customSignals;
	private static bool proximity = false;
	// Called when the node enters the scene tree for the first time.
	int health = 10;
	private PackedScene soldier = ResourceLoader.Load<PackedScene>("res://scenes/soldier.tscn");
	public override void _Ready()
	{
		GD.Print(this.GetPath().ToString());
		var command = GetNode<Area2D>("spawnZone");
		var popup = GetNode<Panel>("popup");
		customSignals = GetNode<Signals>("/root/Signals");

		command.BodyEntered += interaction;
		command.BodyExited += interaction;
		customSignals.enemyDamage += damaged;
		popup.Visible = false;
		proximity = false;
		GetNode<Label>("popup/VBoxContainer/commandHealth").Text = "Command Health: " + health;
		
		//barSprite.RegionEnabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		upgrade();
	}

	public void damaged(string pathid)
	{
		//GD.Print("command damaged getting called");
		//GD.Print("pathid of damaged: " + pathid + " pathid of command: " + this.GetPath().ToString());
		if (pathid == (this.GetPath().ToString() + "/spawnZone"))
		{
			health -= 1;
			GD.Print("enemy hit! health: " + health);
			var healthLabel = GetNode<Label>("popup/VBoxContainer/commandHealth");
			if (health <= 0)
			{
				GD.Print("command destroyed");
				GetTree().Paused = true;
				GetNode<Control>("/root/game/SceneManager/testLevel2/Player/player/Camera2D/levelUI/deathMenu").Show();
			}
			healthLabel.Text = "Command Health: " + health;
		}
	}

	public void upgrade()
	{
		
		if(Input.IsActionJustPressed("upgrade") && proximity == true)
		{
			GD.Print("made it to here");
			var upgrade = GetNode<Label>("popup/VBoxContainer/upgrade");
			var scrap = GetNode<Label>("/root/game/SceneManager/testLevel2/Player/player/Camera2D/levelUI/scrap");
			
			int totalScrap = Int32.Parse(scrap.Text.Substring(7));
			int scrapCost = Int32.Parse(upgrade.Text.Substring(14,2));
			if(totalScrap >= scrapCost )
			{
				customSignals.EmitSignal(nameof(customSignals.resourceModify),scrapCost,false);
			}

			var soldier_instance = soldier.Instantiate();


			(soldier_instance as Node2D).GlobalPosition = GetNode<Marker2D>("/root/game/SceneManager/testLevel2/soldierSpawn").GlobalPosition;
			GetParent().AddChild(soldier_instance);
		}
	}

	public void interaction(Node2D body)
	{
		var command = GetNode<Sprite2D>("commandSprite");
		var popup = GetNode<Panel>("popup");

		GD.Print("interaction has been called, proximity is: " + proximity);
		if(!proximity)
		{
			proximity = true;
			popup.Visible = true;
			GD.Print("popup visibility is: " + popup.Visible);
		}
		else
		{
			proximity = false;
			popup.Visible = false;
			GD.Print("popup visibility is: " + popup.Visible);
		}
	}
}
