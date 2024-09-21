using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float direction = Input.GetAxis("left","right");
		AnimatedSprite2D playerSprite = GetNode<AnimatedSprite2D>("playerBody");
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		// flips the sprite based on which direction the player is moving
		if(direction > 0)
			playerSprite.FlipH = false;
		else if(direction < 0)
			playerSprite.FlipH = true;

		if(direction != 0)
			playerSprite.Play("walk");
		else if(Input.IsActionPressed("shoot"))
			{
				playerSprite.Play("attack");
				GD.Print("attacking\n");
			}
		else
			{
				playerSprite.Play("idle");
				GD.Print("idling\n");
			}

		if (direction != 0)
		{
			
			velocity.X = direction * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
