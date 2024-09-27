using Godot;
using System;

public partial class Crystal : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private bool proximity = false;

	public void interaction(Node2D body)
	{
		GD.Print("interaction has been called, proximity is: " + proximity);
		if(proximity)
			proximity = false;
		else
			proximity = true;
	}
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var material = GetNode<Sprite2D>("crystalSprite").Material;
		 if(!proximity)
		 {
			(material as ShaderMaterial).SetShaderParameter("onoff", 1);
			GD.Print("shader is on switch is:" + (material as ShaderMaterial).GetShaderParameter("onoff"));
			
		 }
		else
		 	(material as ShaderMaterial).SetShaderParameter("onoff", 0);
	}
}
