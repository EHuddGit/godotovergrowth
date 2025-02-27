using Godot;
using System;

public partial class Menu : Control
{
	// Called when the node enters the scene tree for the first time.
	private PackedScene nextScene = (PackedScene)ResourceLoader.Load("res://scenes/testLevel.tscn");
	private PackedScene settingsPage = (PackedScene)ResourceLoader.Load("res://scenes/settings.tscn");
	public override void _Ready()
	{
		var start = GetNode<Button>("VBoxContainer/Start");
		var settings = GetNode<Button>("VBoxContainer/Settings");
		start.Pressed += StartGame;
		settings.Pressed += SettingsPage;
		GD.Print(this.GetPath().ToString());
	}

	public void SettingsPage()
	{
		GetTree().ChangeSceneToPacked(settingsPage);
		var scene_instance = nextScene.Instantiate();
		GetParent().AddChild(scene_instance);
		this.QueueFree();

	}
	public void StartGame()
	{
        GetTree().ChangeSceneToPacked(nextScene);
		GD.Print(this.GetPath().ToString());
		var scene_instance = nextScene.Instantiate();
		GD.Print(GetParent().GetPath().ToString());
		var targetNode = GetNode<Node2D>("SceneManager");
		targetNode.AddChild(scene_instance);
		//this.QueueFree();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
}
