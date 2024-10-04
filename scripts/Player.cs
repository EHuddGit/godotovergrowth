using Godot;
using System;
using System.Collections.Generic;

//need to move all animations to process and take them out of physics process
// timing of the sprite shooting the bullet and the bullet coming out seems off
//bullet sprite needs to be flipped
// will probably have to change when spawning bullet gets unlocked to run instead of signal when the animation is done
// will have to change bullet's root node to something that can use collisions
public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
    enum States {IDLE,WALKING,SHOOTING,RUNNING, COMMANDING};
	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");

	//flags
	private States current = States.IDLE;
	private bool finishedShot = false;
	private bool finishedCommand = false;
	private bool bulletFired = false;
	private bool followed = false;
	private bool signal = false;
	private Sprite2D nearbyObject;

	private List<AnimatedSprite2D> followers;
	private Signals customSignals;

	//----------------------------signals---------------------------------------
	public void soldierFollowing(AnimatedSprite2D follower)
	{
		followed = true;
		GD.Print("player has gained a follower");
		
	}
	public void objectClose(Sprite2D interactable)
	{
		nearbyObject = interactable;
		GD.Print("object nearby");
	}
	public void animationFinish()
	{
		if(current == States.SHOOTING)
		{
			GD.Print("shooting flags");
			finishedShot = true;
			if(bulletFired == true)
				bulletFired = false;
		}
		else if(current == States.COMMANDING)
		{
			GD.Print("commanding flags");
			finishedCommand = true;
			signal = false;
		}
	}

//---------------------------loop functions----------------------------------------------
	public override void _Ready()
	{
		Global.playerInstance = GetNode<AnimatedSprite2D>("playerBody");
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.followingPlayer += soldierFollowing;
		customSignals.objectNearby += objectClose;
		
	}

	public override void _PhysicsProcess(double delta)
	{
		//Engine.TimeScale = 0.25;
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
	//this function should also be running in the process function
	public void stateChange(float direction)
	{
		
		if(direction != 0 && ((current == States.SHOOTING && finishedShot == true) || current != States.SHOOTING))
			current = States.WALKING;

		else if(Input.IsActionPressed("shoot"))
			current = States.SHOOTING;

		else if(Input.IsActionJustPressed("command"))
		{
			current = States.COMMANDING;
			if(!signal)
			{
				if(followed)
				{
					if(nearbyObject != null)
					{
						if(GetNode<AnimatedSprite2D>("playerBody").GlobalPosition.X < nearbyObject.GlobalPosition.X + 150 &&
						GetNode<AnimatedSprite2D>("playerBody").GlobalPosition.X > nearbyObject.GlobalPosition.X - 150 )
						{
							GD.Print("commanding to mine");
							customSignals.EmitSignal(nameof(customSignals.playerCommandingMine),nearbyObject);
						}
					}
					followed = false;
				}
				else
					customSignals.EmitSignal(nameof(customSignals.playerCommanding));
				signal = true;
			}
			
		}
		else
		{
			if(current != States.SHOOTING && current != States.COMMANDING)
				current = States.IDLE;
			else if(current == States.SHOOTING && finishedShot == true)
			{
				current = States.IDLE;
				finishedShot = false;
				bulletFired = false;
			}
			else if(current == States.COMMANDING && finishedCommand == true)
			{
			 	current = States.IDLE;
				finishedCommand = false;
			}
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
		else if(current == States.COMMANDING)
			playerSprite.Play("command");
		else if(current == States.WALKING)
			playerSprite.Play("walk");
		else
			playerSprite.Play("idle");
	}
	
	//------------------------helper functions-------------------------------------------
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
	
	
}
