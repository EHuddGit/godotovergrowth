using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

//need to move all animations to process and take them out of physics process
// timing of the sprite shooting the bullet and the bullet coming out seems off
//bullet sprite needs to be flipped
// will probably have to change when spawning bullet gets unlocked to run instead of signal when the animation is done
// will have to change bullet's root node to something that can use collisions
public partial class Player : CharacterBody2D
{
	[Signal]
	public delegate void TestSignalEventHandler(int value);
	public const float Speed = 150.0f;
	public const float JumpVelocity = -400.0f;
	enum States { IDLE, WALKING, SHOOTING, RUNNING, COMMANDING };
	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");

	//flags
	private States current = States.IDLE;
	private bool finishedShot = false;
	private bool finishedCommand = false;
	private bool bulletFired = false;
	private bool signal = false;
	private Sprite2D nearbyObject;
	private Godot.Collections.Array<bool> objectFollowerSpots;
	private List<string> followers;
	private Signals customSignals;

	//----------------------------signals---------------------------------------


	public void soldierFollowing(string pathid)
	{
		followers.Add(pathid);
		GD.Print("player has gained a follower");
		GD.Print("follower path:" + pathid);

	}
	public void objectClose(Sprite2D interactable, Godot.Collections.Array<bool> followerSpots)
	{
		nearbyObject = interactable;
		objectFollowerSpots = followerSpots;
		GD.Print("object nearby");
	}
	public void animationFinish()
	{
		if (current == States.SHOOTING)
		{
			GD.Print("shooting flags");
			finishedShot = true;
			if (bulletFired == true)
				bulletFired = false;
		}
		else if (current == States.COMMANDING)
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
		followers = new List<string>();

	}

	public override void _PhysicsProcess(double delta)
	{
		//Engine.TimeScale = 0.25;
		Vector2 velocity = Velocity;
		float direction = Input.GetAxis("left", "right");

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		stateChange(direction);
		animations(direction);
		if (current == States.SHOOTING)
		{
			bulletspawn();
			EmitSignal(nameof(TestSignal), 10);
		}
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		GD.Print("velocity:" + Velocity);
		MoveAndSlide();
	}


	//this function should also be running in the process function
	//need to try and have stateChange only be about states and remove clutter that can be its own functions
	//shooting should try and be its own function
	public void stateChange(float direction)
	{

		if (direction != 0 && ((current == States.SHOOTING && finishedShot == true) || current != States.SHOOTING))
			current = States.WALKING;

		else if (Input.IsActionPressed("shoot"))
			current = States.SHOOTING;

		else if (Input.IsActionJustPressed("command"))
		{
			current = States.COMMANDING;
			playerCommand();
		}
		else
		{
			if (current != States.SHOOTING && current != States.COMMANDING)
				current = States.IDLE;
			else if (current == States.SHOOTING && finishedShot == true)
			{
				current = States.IDLE;
				finishedShot = false;
				bulletFired = false;
			}
			else if (current == States.COMMANDING && finishedCommand == true)
			{
				current = States.IDLE;
				finishedCommand = false;
			}
		}

	}

	public void playerCommand()
	{
		GD.Print("amount of followers: " + followers.Count);
		Signals.COMMANDS comm = Signals.COMMANDS.GUARDING;
		int index = 0;

		if(nearbyObject == null)
		{
			GD.Print("object is null");
		}
		if (nearbyObject != null && followers.Count > 0)
		{
			GD.Print("made it to here1");
			if (GetNode<AnimatedSprite2D>("playerBody").GlobalPosition.X < nearbyObject.GlobalPosition.X + 150 &&
			GetNode<AnimatedSprite2D>("playerBody").GlobalPosition.X > nearbyObject.GlobalPosition.X - 150)
			{
				GD.Print("made it to here2");
				if(nearbyObject.GetPath().ToString().Contains("Crystal"))
				{
					comm = Signals.COMMANDS.MINING;
				}
				else if(nearbyObject.GetPath().ToString().Contains("Barricade"))
				{
					comm = Signals.COMMANDS.GUARDING;
				}
				

				while(objectFollowerSpots[index] != false || index >= objectFollowerSpots.Count)
				{
					index++;
				}
				
				GD.Print("player told soldier to mine/guard");
				customSignals.EmitSignal(nameof(customSignals.playerCommandingObject),followers[0], nearbyObject,(int)comm, (index + 1) * 75);
				followers.Remove(followers[0]);
			}
		}
		else
		{

			GD.Print("follow/wander command");
			customSignals.EmitSignal(nameof(customSignals.playerCommanding));
		}


	}

	public void animations(float direction)
	{
		AnimatedSprite2D playerSprite = GetNode<AnimatedSprite2D>("playerBody");
		Marker2D muzzle = GetNode<Marker2D>("muzzle");
		// flips the sprite based on which direction the player is moving
		if (direction > 0)
		{
			playerSprite.FlipH = false;
		}
		else if (direction < 0)
		{
			playerSprite.FlipH = true;
		}

		//playing which animation based on the state
		if (current == States.SHOOTING)
			playerSprite.Play("attack");
		else if (current == States.COMMANDING)
			playerSprite.Play("command");
		else if (current == States.WALKING)
			playerSprite.Play("walk");
		else
			playerSprite.Play("idle");
	}

	//------------------------helper functions-------------------------------------------
	public void bulletspawn()
	{
		int direction = 0;
		if (bulletFired == false)
		{
			GD.Print("bullet spawned!\n");
			Marker2D muzzle = GetNode<Marker2D>("muzzle");
			Vector2 muzzlePosition = muzzle.GlobalPosition;
			var bullet_instance = bullet.Instantiate();

			GD.Print(GetNode<AnimatedSprite2D>("playerBody").FlipH);
			if (GetNode<AnimatedSprite2D>("playerBody").FlipH)
			{
				direction = -1;
				GD.Print(muzzlePosition.X);
				muzzlePosition.X -= 1.75F * muzzle.Position.X;
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
