using Godot;
using System;

public partial class LevelUi : Control
{
	// Called when the node enters the scene tree for the first time.
	private Signals customSignals;
	int storage;
	private Label scrapTotal;
	public override void _Ready()
	{
		scrapTotal = GetNode<Label>("scrap");
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.resourceModify += resourceChange;
		scrapTotal.Text = "Scrap: 0";
	
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
}
