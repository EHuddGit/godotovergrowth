using Godot;
using System;
using System.Threading;

public partial class Soldier : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;
	public const float WanderDistance = 100.0f;
	public const float objectRange = 50.0f;
	public const float playerY = 586.0f;
	public const float resourceY = 546.0f;
	private PackedScene bullet = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");

	enum States { IDLE, WANDER, FOLLOW, SHOOT, GATHER, SEEK, GUARD, DYING };
	enum WanderStates { IDLE, MOVING };
	enum CommandStates { FOLLOW, MINE, GAURD, NONE };
	public enum WanderDirections { LEFT, RIGHT, BOTH };

	static float initialPosition = 0;
	float offset = 0;
	float futurePosition = 0;
	int health = 6;
	bool following = false;
	bool inRange = false;
	bool isWandering = false;
	bool isGathering = false;
	bool enemyDetected = false;
	bool bulletFired = false;
	bool animationFinish = false;
	float xdirection = 0;
	float ydirection = 0;
	bool ychange = false;
	bool registered = false;
	bool commanded = false;
	States current = States.WANDER;
	WanderStates wanderCurrent = WanderStates.MOVING;
	CommandStates commandCurrent = CommandStates.NONE;
	private Signals customSignals;

	Sprite2D obj;



	public override void _Ready()
	{
		customSignals = GetNode<Signals>("/root/Signals");
		customSignals.playerCommanding += playerCommandFollow;
		customSignals.playerCommandingObject += playerCommandObject;
		customSignals.enemyDamage += damaged;
		initialPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
		var bulletTimer = GetNode<Godot.Timer>("shootTimer");
		bulletTimer.Timeout += bulletspawn;
		GD.Print(this.GetPath());
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		enemyRayCast();
		movement(velocity, delta);
		animations();

	}

	public void enemyRayCast()
	{
		var raycast = GetNode<RayCast2D>("enemyTriggerRayCast");
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");

		if (soldier.FlipH && raycast.RotationDegrees != 180)
		{
			//GD.Print("zero degrees");
			raycast.RotationDegrees = 180;
		}
		else if (!soldier.FlipH && raycast.RotationDegrees != 0)
		{
			raycast.RotationDegrees = 0;
			//	GD.Print("180 degrees");
		}
		//GD.Print(raycast.RotationDegrees);

		if (raycast.IsColliding())
		{
			enemyDetected = true;
			//GD.Print("collison detected!");
		}
		else if (enemyDetected)
			enemyDetected = false;
	}

	//a signal function that is triggered by a player sending a command to a soldier following the player
	//depending on the object the soldier will either mine or guard
	public void playerCommandObject(string pathID, Sprite2D Obj, Signals.COMMANDS command, float Offset)
	{
		GD.Print("soldier commanded to an object");
		GD.Print("soldier state: " + current);
		if (current == States.FOLLOW && this.GetPath() == pathID)
		{
			obj = Obj;
			if(command == Signals.COMMANDS.WANDERING)
			{
				GD.Print("player has commanded soldier to wander");
				current = States.WANDER;
				initialPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
			}
			else if (command == Signals.COMMANDS.MINING)
			{
				GD.Print("soldier is mining");
				current = States.GATHER;
			}
			else if (command == Signals.COMMANDS.GUARDING)
			{
				GD.Print("soldier is guarding");
				current = States.GUARD;

				if (Obj.GlobalPosition.X > 0)
					offset = -Offset;
				else
					offset = Offset;

				//initialPosition = Obj.GlobalPosition.X + Offset;
				GD.Print("command offset: " + Offset);
				GD.Print("object position:" + Obj.GlobalPosition.X + Offset);
			}

		}
	}
	public void playerCommandFollow()
	{
		GD.Print("soldier recieved command");
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		//turns off the wandering or gathering animations
		isGathering = false;

		if (inRange)
		{
			isWandering = false;
			if (current != States.FOLLOW)
			{
				current = States.FOLLOW;
				GD.Print("player has commanded soldier to follow");
				customSignals.EmitSignal(nameof(customSignals.followingPlayer), this.GetPath());
			}

			GD.Print("status: " + current);
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


	public void playerEntered(Node2D body)
	{
		var material = GetNode<AnimatedSprite2D>("soldierBody").Material;
		var area = GetNode<Area2D>("commandArea");
		GD.Print("this entered or exited: " + body.GetPath().ToString());
		var bodies = area.GetOverlappingBodies();
		bool change = false;
		foreach (var bodyin in bodies)
		{
			if(bodyin == body)
			{
				change = true;
				GD.Print("player near soldier");
			}
		}

		if(change)
			inRange = true;
		else
			inRange = false;

		if (inRange)
		{
			//inRange = false;
			(material as ShaderMaterial).SetShaderParameter("onoff", 1);
		}
		else
		{
			//inRange = true;
			//if (!following)
				(material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
	}


	public void animations()
	{
		var body = GetNode<AnimatedSprite2D>("soldierBody");
		if (xdirection > 0)
		{
			body.FlipH = false;
		}
		else if (xdirection < 0)
		{
			body.FlipH = true;
		}

		if (current == States.DYING)
		{
			body.Play("dead");

			if(!body.FlipH)
				body.Offset = new Vector2(56,0);
			else
				body.Offset = new Vector2(-71,0);
		}
		else if (xdirection != 0)
		{
			body.Play("walk");

			if(!body.FlipH)
				body.Offset = new Vector2(-8,0);
			else
				body.Offset = new Vector2(-6,0);

		}
		else if (current == States.SHOOT)
		{
			body.Play("shoot");
			var bulletTimer = GetNode<Godot.Timer>("shootTimer");
			if(!bulletFired)
			{
				bulletTimer.Start(0.4F);
				bulletFired = true;
			}
			GD.Print("time left: " + bulletTimer.TimeLeft);

			if(!body.FlipH)
				body.Offset = new Vector2(10,0);
			else
				body.Offset = new Vector2(-25,0);

		}
		else if (isGathering)
		{
			body.Play("gather");

			if(!body.FlipH)
				body.Offset = new Vector2(-1,0);
			else
				body.Offset = new Vector2(-14,0);

		}
		else
		{
			body.Play("idle");

			
			if(!body.FlipH)
				body.Offset = new Vector2(-13,0);
			else
				body.Offset = new Vector2(-2,0);
		}
	}

	public void animationFinished()
	{
		if (current == States.SHOOT)
		{
			animationFinish = true;
			if (bulletFired == true)
				bulletFired = false;
		}
		else if (current == States.DYING)
		{
			customSignals.playerCommanding -= playerCommandFollow;
			customSignals.playerCommandingObject -= playerCommandObject;
			customSignals.enemyDamage -= damaged;
			this.CollisionLayer = 0;
			this.QueueFree();
		}

	}

	public void movement(Vector2 velocity, double delta)
	{
		int[] distances = { 25, 50, 75 };
		int[] directionChoice = { -1, 1 };
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;

		// if (!IsOnFloor())
		// {
		// 	velocity += GetGravity() * (float)delta;
		// }
		//where states are checked on
		if (current == States.DYING)
			xdirection = 0;
		else if (current == States.WANDER)
			wandering();
		else if (current == States.FOLLOW)
			follow();
		else if (current == States.GATHER)
			gather();
		else if (current == States.SHOOT)
			shoot();
		else if (current == States.GUARD)
			guard();



		if (xdirection != 0)
			velocity.X = xdirection * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}


	public void shoot()
	{
		xdirection = 0;

		if (current != States.SHOOT && enemyDetected)
		{
			current = States.SHOOT;
			//GD.Print("enemy detected");
		}
		if (current == States.SHOOT && !enemyDetected)
			current = States.GUARD;

		//bulletspawn();

	}

	public void bulletspawn()
	{
		GD.Print("bullet spawned");
		int direction = 0;
		//if (bulletFired == false)
		//{
			GD.Print("bullet spawned!\n");
			Marker2D muzzleR = GetNode<Marker2D>("bulletspawnR");
			Marker2D muzzleL = GetNode<Marker2D>("bulletspawnL");
			Vector2 muzzlePosition;
			var bullet_instance = bullet.Instantiate();

			GD.Print(GetNode<AnimatedSprite2D>("soldierBody").FlipH);
			if (GetNode<AnimatedSprite2D>("soldierBody").FlipH)
			{
				direction = -1;
				muzzlePosition = muzzleL.GlobalPosition;
			}
			else
			{
				direction = 1;
				muzzlePosition = muzzleR.GlobalPosition;
			}
			(bullet_instance as Bullet).direction = direction;

			(bullet_instance as Node2D).GlobalPosition = muzzlePosition;
			GetParent().AddChild(bullet_instance);
			//bulletFired = true;
		//}
	}

	public void guard()
	{
		if (seek(10))
		{
			xdirection = 0;
			var body = GetNode<AnimatedSprite2D>("soldierBody");
			float facing = body.GlobalPosition.X - (obj.GlobalPosition.X + offset);

			if (obj.GlobalPosition.X < 0 && facing < 0 && body.FlipH == false)
				body.FlipH = true;
			else if (obj.GlobalPosition.X > 0 && facing > 0 && body.FlipH == true)
				body.FlipH = false;

			if (enemyDetected)
				current = States.SHOOT;
			else if (!enemyDetected && current == States.SHOOT)
				current = States.GUARD;
		}
	}

	public bool seek(float objRange = objectRange)
	{
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		bool withinRange = false;
		float target = obj.GlobalPosition.X + offset;
		//GD.Print("offset: " + offset);
		if(soldier.GlobalPosition.Y < target - objRange && soldier.GlobalPosition.Y < target + objRange)
		{
			ydirection = 1;
		}
		if (soldier.GlobalPosition.X < target - objRange && soldier.GlobalPosition.X < target + objRange)
		{
			xdirection = 2;
			withinRange = false;
		}
		else if (soldier.GlobalPosition.X > target - objRange && soldier.GlobalPosition.X > target + objRange)
		{
			xdirection = -2;
			withinRange = false;
		}
		else
		{
			withinRange = true;
		}

		return withinRange;

	}

	public void gather()
	{
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		var timer = GetNode<Godot.Timer>("GatherTimer");
		if (seek())
		{
			xdirection = 0;
			if (!isGathering)
			{
				isGathering = true;
				timer.Start(3);
				GD.Print("gathering started!");
			}
			else
			{
				if (timer.IsStopped())
				{
					customSignals.EmitSignal(nameof(customSignals.resourceMined), obj);
					timer.Start(3);
					GD.Print("gathering restarted!");
				}
			}
		}
	}
	public void follow()
	{
		//EmitSignal(SignalName.HealthChanged, oldHealth, _health);
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		//EmitSignal(SignalName.Follower,soldier);

		if (!inRange)
		{
			if (Global.playerInstance.GlobalPosition.X > soldier.GlobalPosition.X)
				xdirection = 2;
			else
				xdirection = -2;
		}
		else
			xdirection = 0;
	}

	//its stuttering when choosing to move twice
	public void wandering(WanderDirections directions = WanderDirections.BOTH, float distance = WanderDistance)
	{

		var timer = GetNode<Godot.Timer>("wanderTimer");
		int[] distances = { 25, 50, 75 };
		int[] directionChoice = { -1, 1 };
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
		bool withinL = true;
		bool withinR = true;


		if (wanderCurrent == WanderStates.IDLE)
		{
			//GD.Print("i am idle");
			xdirection = 0;
			if (!isWandering)
			{
				timer.Start(GD.Randi() % 2 + 1);
				isWandering = true;
			}
			else if (timer.IsStopped())
			{
				//GD.Print("timer stopped");
				wanderCurrent = WanderStates.MOVING;
				isWandering = false;
			}
		}
		else if (wanderCurrent == WanderStates.MOVING)
		{
			//GD.Print("i am moving");
			if (!isWandering)
			{

				xdirection = directionChoice[GD.Randi() % 2];
				tempDistance = distances[GD.Randi() % 3];
				// // checks if the soldier's next move is within allowed range on the left or right
				// if((tempDistance * direction) + currentPosition > initialPosition + distance)
				// 	withinR = false;

				// if((tempDistance * direction) + currentPosition < initialPosition + distance)
				// 	withinL = false;

				switch (directions)
				{
					case WanderDirections.LEFT:
						if (withinL == false || (tempDistance * xdirection) + currentPosition < initialPosition)
						{
							xdirection = -xdirection;
							GD.Print("direction change!");
						}
						break;
					case WanderDirections.RIGHT:
						if (withinR == false || (tempDistance * xdirection) + currentPosition > initialPosition)
							xdirection = -xdirection;
						break;
					case WanderDirections.BOTH:
						if (withinR == false || withinL == false)
							xdirection = -xdirection;
						break;
				}
				//GD.Print("global position: " + this.GlobalPosition.X);
				futurePosition = currentPosition + (tempDistance * xdirection);
				//GD.Print("current position: " + currentPosition + " future positon: " + futurePosition + " object position: " + initialPosition);
				//GD.Print("current position: " + currentPosition +" next position: " + futurePosition);
				isWandering = true;

			}
			else
			{
				if ((xdirection == -1 && currentPosition <= futurePosition) ||
				(xdirection == 1 && currentPosition >= futurePosition))
				{
					//GD.Print("going back to idle")
					isWandering = false;
					wanderCurrent = WanderStates.IDLE;

				}

			}
		}

	}

}
