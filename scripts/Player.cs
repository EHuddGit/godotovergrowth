using Godot;
using System;

//need to move all animations to process and take them out of physics process
// timing of the sprite shooting the bullet and the bullet coming out seems off
//bullet sprite needs to be flipped
public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
    enum States {IDLE,WALKING,SHOOTING,RUNNING};
	private States current = States.IDLE;
	private bool finishedShot = false;
	private bool bulletFired = false;
	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");
	public void stateChange(float direction)
	{
		
		if(direction != 0 && ((current == States.SHOOTING && finishedShot == true) || current != States.SHOOTING))
			current = States.WALKING;
		else if(Input.IsActionPressed("shoot"))
			current = States.SHOOTING;
		else if((current == States.SHOOTING && finishedShot == true) || current != States.SHOOTING)
		{
			current = States.IDLE;
			finishedShot = false;
		}
	}

	public void animations(float direction)
	{
		AnimatedSprite2D playerSprite = GetNode<AnimatedSprite2D>("playerBody");
		Marker2D muzzle = GetNode<Marker2D>("muzzle");
		// flips the sprite based on which direction the player is moving
		if(direction > 0)
		{
			playerSprite.FlipH = false;
		}
		else if(direction < 0)
		{
			playerSprite.FlipH = true;
		}
		//playing which animation based on the state
		if(current == States.SHOOTING)
			playerSprite.Play("attack");
		else if(current == States.WALKING)
			playerSprite.Play("walk");
		else
			playerSprite.Play("idle");
	}
	public void animationFinish()
	{
		finishedShot = true;
		if(bulletFired == true)
			bulletFired = false;
		GD.Print("animation is finished\n");
	}
	public void bulletspawn()
	{
		int direction = 0;
		if(bulletFired == false)
		{
			GD.Print("bullet spawned!\n");
			Marker2D muzzle = GetNode<Marker2D>("muzzle");
			Vector2 muzzlePosition = muzzle.GlobalPosition;
			var bullet_instance = bullet.Instantiate();

			GD.Print(GetNode<AnimatedSprite2D>("playerBody").FlipH);
			if(GetNode<AnimatedSprite2D>("playerBody").FlipH)
			{
				direction = -1;
				GD.Print(muzzlePosition.X);
				muzzlePosition.X  -= 1.75F * muzzle.Position.X;
			}
			else
			{
				direction = 1;
				GD.Print(muzzlePosition.X);
			}
			(bullet_instance as Bullet).direction = direction;

			(bullet_instance as Node2D).GlobalPosition = muzzlePosition;
			GetParent().AddChild(bullet_instance);
			bulletFired = true;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float direction = Input.GetAxis("left","right");
		
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		stateChange(direction);
		animations(direction);
		if(current == States.SHOOTING)
			bulletspawn();

		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}

	// public override void _PhysicsProcess(double delta)
	// {
	// 	//input to signal state change
	// 	//change the state if needed
	// 	//process the appropiate actions for current state
	// }
	
}
