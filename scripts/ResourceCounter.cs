using Godot;
using System;

public partial class ResourceCounter : Label
{
	// Called when the node enters the scene tree for the first time.
	private Signals customSignals;
	int storage;
	public override void _Ready()
	{
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.resourceModify += resourceChange;
		this.Text = "Crystals: 0";
	
	}

	public void resourceChange(int amount, bool isAdd)
	{
		if(isAdd)
			storage += amount;
		else
			storage -= amount;
		this.Text = "Crystals: " + storage;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
