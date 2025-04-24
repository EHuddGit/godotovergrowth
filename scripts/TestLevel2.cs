using Godot;
using System;
using System.Globalization;

public partial class TestLevel2 : Node2D
{ // add wave timers, update the ui timer node, spawn enemy at points specified
	// Called when the node enters the scene tree for the first time.
	private Timer waves;
	private static int waveTimeLength = 25;
	private int countDown = waveTimeLength;
	private PackedScene enemy = ResourceLoader.Load<PackedScene>("res://scenes/enemy.tscn");
	private static int enemyNumber = 1;
	private static int waveNumber = 1;

	private bool wavePaused = false;

	public override void _Ready()
	{
		waves = GetNode<Timer>("timers/waveTimer");
		var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
		//waves.Autostart = true;
		waves.OneShot = false;
		waves.WaitTime = 1.0f;
		waves.Timeout += reset;
		timeLabel.Text = "next wave: " + countDown;
	}

	public override void _Process(double delta)
	{
		
		if (Input.IsActionJustReleased("pauseWave"))//Input.IsActionPressed("pauseWave"))
		{
			GD.Print("p is pressed");
			wavePaused = !wavePaused;
		}
	}
	
	// public void waveTime()
	// {
	// 	var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
	// 	timeLabel.Text = "Timer: " + (int)waves.TimeLeft;
	// }

	public void reset()
	{
		waves.Start(1.0f);
		if(!wavePaused)
		{
			var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
			timeLabel.Text = "next wave: " + countDown;
		
			//GD.Print("countDown: " + countDown);

			if(countDown == 0)
			{
				if(enemyNumber != 0)
				{
					spawnEnemy();
					enemyNumber--;
				}
				else
				{
					countDown = waveTimeLength;
					waveNumber += 1;
					enemyNumber = 1 + (waveNumber * 2);
				}
			}
			else
				countDown -= 1;
		}

	}
	
	
	

	public void spawnEnemy()
	{
		var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
		var spawnpointR = GetNode<Marker2D>("enemySpawnR");
		var spawnpointL = GetNode<Marker2D>("enemySpawnL");

		if(countDown == 0)
		{
			//countDown = waveTimeLength;
			Vector2 spawnpointPosition;
			var random = GD.Randi() % 2 + 1;
			//GD.Print("number is: " + random);

			if(random == 1)
			{
				spawnpointPosition = spawnpointR.GlobalPosition;
				GD.Print("spawning on the right");
			}
			else
			{
				spawnpointPosition = spawnpointL.GlobalPosition;
				GD.Print("spawning on the left");
			}

			
			var enemy_instance = enemy.Instantiate();
			(enemy_instance as Node2D).GlobalPosition = spawnpointPosition;
			(enemy_instance as Node2D).Scale = new Vector2(2.0F,2.0F);
			GetNode<Node>("enemies").AddChild(enemy_instance);
			
		}

	}
}
