using Godot;
using System;

public partial class LevelUi : Control
{
	// Called when the node enters the scene tree for the first time.
	private Signals customSignals;
	int storage;
	private Label scrapTotal;
	private Button pause;
	private Control pauseMenu;
	private PackedScene menu = (PackedScene)ResourceLoader.Load("res://scenes/menu.tscn");
	public override void _Ready()
	{
		scrapTotal = GetNode<Label>("scrap");
		customSignals = GetNode<Signals>("/root/Signals");
		pause = GetNode<Button>("pause");
		pauseMenu = GetNode<Control>("pauseMenu");
		var resumeButton = GetNode<Button>("pauseMenu/pauseMenuBox/resume");

		customSignals.resourceModify += resourceChange;
		scrapTotal.Text = "Scrap: 0";
		pause.Pressed += paused;
		pauseMenu.Hide();
		resumeButton.Pressed += resume;
		GetNode<Button>("pauseMenu/pauseMenuBox/exit").Pressed += exit;

	
	}

	public void resourceChange(int amount, bool isAdd)
	{
		if(isAdd)
			storage += amount;
		else
			storage -= amount;

		scrapTotal.Text = "Scrap: " + storage;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void paused()
	{
		GetTree().Paused = true;
		pauseMenu.Show();
	}
	public void resume()
	{
		pauseMenu.Hide();
		GetTree().Paused = false;
	}
	public void exit()
	{
		GetTree().Paused = false;
		var scene_instance = menu.Instantiate();
		var targetParentNode = GetNode<Node>("/root/game/SceneManager");
		var targetNode = GetNode<Node2D>("/root/game/SceneManager/testLevel2");
		GD.Print("target node: " + targetNode.GetPath());
		targetParentNode.AddChild(scene_instance);
		targetNode.QueueFree();
	}
	
}
