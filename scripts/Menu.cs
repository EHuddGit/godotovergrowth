using Godot;
using System;

public partial class Menu : Control
{
	// Called when the node enters the scene tree for the first time.
	private PackedScene nextScene = (PackedScene)ResourceLoader.Load("res://scenes/testLevel2.tscn");
	private PackedScene settingsPage = (PackedScene)ResourceLoader.Load("res://scenes/settings.tscn");
	private PackedScene scorePage = (PackedScene)ResourceLoader.Load("res://scenes/scoreBoard.tscn");
	public override void _Ready()
	{
		var start = GetNode<Button>("VBoxContainer/Start");
		var settings = GetNode<Button>("VBoxContainer/Settings");
		var score = GetNode<Button>("VBoxContainer/Score");
		start.Pressed += StartGame;
		settings.Pressed += SettingsPage;
		score.Pressed += ScorePage;
		GD.Print(this.GetPath().ToString());
	}

	public void ScorePage()
	{
		var scene_instance = scorePage.Instantiate();
		var targetNode = GetParent();
		targetNode.AddChild(scene_instance);
		this.QueueFree();
	}
	public void SettingsPage()
	{
		var scene_instance = settingsPage.Instantiate();
		var targetNode = GetParent();
		targetNode.AddChild(scene_instance);
		this.QueueFree();

	}
	public void StartGame()
	{
		var scene_instance = nextScene.Instantiate();
		var targetNode = GetParent();
		targetNode.AddChild(scene_instance);
		this.QueueFree();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
}
