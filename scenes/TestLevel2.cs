using Godot;
using System;

public partial class TestLevel2 : Node2D
{ // add wave timers, update the ui timer node, spawn enemy at points specified
	// Called when the node enters the scene tree for the first time.
	private Timer waves;
	private const int waveTimeLength = 55;
	private int countDown = waveTimeLength;
	private PackedScene enemy = ResourceLoader.Load<PackedScene>("res://scenes/enemy.tscn");
	
	public override void _Ready()
	{
		waves = GetNode<Timer>("timers/waveTimer");
		var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
		//waves.Autostart = true;
		waves.OneShot = false;
		waves.WaitTime = 1.0f;
		waves.Timeout += reset;
		timeLabel.Text = "Timer: " + countDown;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void waveTime()
	{
		var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
		timeLabel.Text = "Timer: " + (int)waves.TimeLeft;
	}

	public void reset()
	{
		var timeLabel = GetNode<Label>("Player/player/Camera2D/levelUI/waveTime");
		var spawnpoint = GetNode<Marker2D>("enemySpawnR");
		countDown -= 1;
		timeLabel.Text = "Timer: " + countDown;

		if(countDown == 0)
		{
			countDown = waveTimeLength;
			Vector2 spawnpointPosition = spawnpoint.GlobalPosition;
			var enemy_instance = enemy.Instantiate();
			(enemy_instance as Node2D).GlobalPosition = spawnpointPosition;
			//GetNode<Node2D>("enemies").AddChild(enemy_instance);
			GetNode<Node>("enemies").AddChild(enemy_instance);
			//this.AddChild(enemy_instance);
		}

		waves.Start(1.0f);
	}
}
