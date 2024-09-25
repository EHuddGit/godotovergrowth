using Godot;
using System;

public partial class Bullet : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private int speed = 400;
	[Export]
	public float direction;


	public override void _PhysicsProcess(double delta)
	{
		if(direction > 0)
		{
			GetNode<AnimatedSprite2D>("bulletBody").FlipH = false;
		}
		else if(direction < 0)
		{
			GetNode<AnimatedSprite2D>("bulletBody").FlipH = true;
		}
		MoveLocalX(direction * speed * (float)delta);
	}
}
