using Godot;
using System;

public partial class Bullet : Node2D
{
	// Called when the node enters the scene tree for the first time.
	private int speed = 400;
	[Export]
	public float direction;
	public float initialPosition;
	public AnimatedSprite2D bullet;

    public override void _Ready()
    {
        
		bullet = GetNode<AnimatedSprite2D>("bulletBody");
		var area = GetNode<Area2D>("hitarea");
		//area.BodyEntered += enemyHit;
		initialPosition = bullet.GlobalPosition.X;
    }




    public override void _PhysicsProcess(double delta)
	{
		if(direction > 0)
		{
			bullet.FlipH = false;
		}
		else if(direction < 0)
		{
			bullet.FlipH = true;
		}
		MoveLocalX(direction * speed * (float)delta);

		
	}

	public void enemyHit(Node2D body)
	{
		GD.Print("hit enemy!");
		this.QueueFree();
	}
}
