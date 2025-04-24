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
	public const float Speed = 250.0f;
	public const float JumpVelocity = -400.0f;
	enum States { IDLE, WALKING, SHOOTING, RUNNING, COMMANDING, GATHERING , DYING};
	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");

	//flags
	private States current = States.IDLE;
	private bool finishedShot = false;
	private bool finished = true;
	private bool finishedCommand = false;
	private bool bulletFired = false;
	private bool signal = false;
	private Sprite2D nearbyObject;
	private Godot.Collections.Array<bool> objectFollowerSpots;
	private List<string> followers;
	private Signals customSignals;
	int health = 10;

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
			finished = true;
			if (bulletFired == true)
				bulletFired = false;
		}
		else if (current == States.COMMANDING)
		{
			GD.Print("commanding flags");
			finishedCommand = true;
			signal = false;
		}
		else if( current == States.GATHERING)
		{
			finishedCommand = true;
			signal = false;
		}
		else if(current == States.DYING)
		{
			GetTree().Paused = true;
			GetNode<Control>("/root/game/SceneManager/testLevel2/Player/player/Camera2D/levelUI/deathMenu").Show();
		}
	}

	public void damaged(string pathid)
	{
		if (pathid == this.GetPath().ToString())
		{
			health -= 1;
			GD.Print("enemy hit! health: " + health);
			if (health <= 0)
			{
				GD.Print("should die now");
				current = States.DYING;
			}
		}
	}

	//---------------------------loop functions----------------------------------------------
	public override void _Ready()
	{
		var timer = GetNode<Timer>("bulletspawnTimer");
		Global.playerInstance = GetNode<AnimatedSprite2D>("playerBody");
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.followingPlayer += soldierFollowing;
		customSignals.objectNearby += objectClose;
		timer.Timeout += bulletspawn;
		customSignals.enemyDamage += damaged;
		followers = new List<string>();

	}

	public override void _PhysicsProcess(double delta)
	{
		//Engine.TimeScale = 0.25;
		Vector2 velocity = Velocity;
		float direction = Input.GetAxis("left", "right");

		stateChange(direction);
		animations(direction);
		if (current == States.SHOOTING)
		{
			//bulletspawn();
			EmitSignal(nameof(TestSignal), 10);
		}
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}
	public void stateChange(float direction)
	{

		if (direction != 0 && ((current == States.SHOOTING && finishedShot == true) || current != States.SHOOTING))
			current = States.WALKING;

		else if (Input.IsActionPressed("shoot"))
			current = States.SHOOTING;
		else if(Input.IsActionJustPressed("gather"))
		{
			current = States.GATHERING;
			playerCommandFollow();
		}
		else if (Input.IsActionJustPressed("command"))
		{
			current = States.COMMANDING;
			playerCommandTask();
		}
		else if(health <= 0)
			current = States.DYING;
		else
		{
			if (current != States.SHOOTING && current != States.COMMANDING && current != States.GATHERING)
				current = States.IDLE;
			else if (current == States.SHOOTING && finishedShot == true)
			{
				current = States.IDLE;
				finishedShot = false;
				bulletFired = false;
			}
			else if ((current == States.COMMANDING || current == States.GATHERING) && finishedCommand == true)
			{
				current = States.IDLE;
				finishedCommand = false;
			}
		}

	}
	public void playerCommandFollow()
	{
		GD.Print("follow command");
		customSignals.EmitSignal(nameof(customSignals.playerCommanding));
	}
	public void playerCommandTask() //known issue that if two objects are in the same command space, soldier will wander instead
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
			else
			{
				comm = Signals.COMMANDS.WANDERING;
				customSignals.EmitSignal(nameof(customSignals.playerCommandingObject),followers[0],default(Variant),(int)comm,0);
				followers.Remove(followers[0]);
			}
			
		}
		
	}

	private enum offsetSprites {SHOOTING = 1,COMMANDING = 2,GATHERING = 2,WALKING = 3,IDLE = 3};
	public void animations(float direction)
	{
		
		var timer = GetNode<Timer>("bulletspawnTimer");
		AnimatedSprite2D playerSprite = GetNode<AnimatedSprite2D>("playerBody");
		Vector2[] offsets = {new Vector2(-4,-7), new Vector2(2,-13),new Vector2(3,-14)};
		int left = 0;
		
		// flips the sprite based on which direction the player is moving
		if (direction > 0)
		{
			playerSprite.FlipH = false;
			left = 0;
		}
		else if (direction < 0)
		{
			playerSprite.FlipH = true;
			left = 1;
		}

		//playing which animation based on the state
		if(current == States.DYING)
		{
			playerSprite.Play("dying");

			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(40,0);
			else
				playerSprite.Offset = new Vector2(-51,0);
		}
		else if (current == States.SHOOTING)
		{
			playerSprite.Play("attack");

			//playerSprite.Offset = new Vector2(offsets[(int)offsetSprites.SHOOTING][left],0);
			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(-4,0);
			else
				playerSprite.Offset = new Vector2(-7,0);

			if( finished == true)
			{
				timer.Start(0.34F);
				GD.Print("time start");
				finished = false;
			}
					
		}
		else if (current == States.COMMANDING)
		{
			playerSprite.Play("command");

			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(2,0);
			else
				playerSprite.Offset = new Vector2(-13,0);
		}
		else if(current == States.GATHERING)
		{
			playerSprite.Play("gather");
			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(2,0);
			else
				playerSprite.Offset = new Vector2(-13,0);
		}
		else if (current == States.WALKING)
		{
			playerSprite.Play("walk");

			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(3,0);
			else
				playerSprite.Offset = new Vector2(-14,0);
		}
		else
		{
			playerSprite.Play("idle");

			if(!playerSprite.FlipH)
				playerSprite.Offset = new Vector2(3,0);
			else
				playerSprite.Offset = new Vector2(-14,0);
		}
	}

	//------------------------helper functions-------------------------------------------
	public void bulletspawn()
	{
		int direction = 0;
		//if (bulletFired == false)
		//{
			GD.Print("bullet spawned!\n");
			Marker2D muzzleR = GetNode<Marker2D>("muzzleRight");
			Marker2D muzzleL = GetNode<Marker2D>("muzzleLeft");
			Vector2 muzzlePosition;
			var bullet_instance = bullet.Instantiate();

			GD.Print(GetNode<AnimatedSprite2D>("playerBody").FlipH);
			if (GetNode<AnimatedSprite2D>("playerBody").FlipH)
			{
				direction = -1;
				//GD.Print(muzzlePosition.X);
				muzzlePosition = muzzleL.GlobalPosition;
			}
			else
			{
				direction = 1;
				muzzlePosition = muzzleR.GlobalPosition;
				//GD.Print(muzzlePosition.X);
			}
			(bullet_instance as Bullet).direction = direction;

			(bullet_instance as Node2D).GlobalPosition = muzzlePosition;
			GetParent().AddChild(bullet_instance);
			bulletFired = true;

			GD.Print("bullet spawned");
		//}
	}


}
