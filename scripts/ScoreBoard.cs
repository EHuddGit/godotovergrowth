using Godot;
using System;

public partial class ScoreBoard : Control
{
	private PackedScene menu = ResourceLoader.Load<PackedScene>("res://scenes/menu.tscn");
	private PackedScene scoreLabel = ResourceLoader.Load<PackedScene>("res://scenes/scoreLabel.tscn");
	public override void _Ready()
	{
		var backButton = GetNode<Button>("Back");
		var scoreContainer = GetNode<VBoxContainer>("ScrollContainer/PanelContainer/scoreContainer");
		backButton.Pressed += Back;
		Tuple<string,string>[] tempData = {new Tuple<string, string>("test1","0100"), new Tuple<string,string>("test2","0075"), new Tuple<string,string>("test3","0050")}; 
		

		for(int index = 0; index < 3; index++)
		{
			var scoreTag = scoreLabel.Instantiate();
			Label score = (Label)scoreTag.FindChild("score");
			Label name = (Label)scoreTag.FindChild("name");

			score.Text = tempData[index].Item2;
			name.Text = tempData[index].Item1;
			scoreContainer.AddChild(scoreTag);
		}
	}

	public override void _Process(double delta)
	{
	}
	public void Back()
	{
		var scene_instance = menu.Instantiate();
		var targetNode = GetParent();
		targetNode.AddChild(scene_instance);
		this.QueueFree();
	}
}
