using Godot;
using System;

public partial class Score : Label
{
	private Signals customSignals;
	int storage;
	public override void _Ready()
	{
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.scoreModify += scoreChange;
		this.Text = "Score: 0";
	
	}

	public void scoreChange(int amount, bool isAdd)
	{
		if(isAdd)
			storage += amount;
		else
			storage -= amount;
		this.Text = "Score: " + storage;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
