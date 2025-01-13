using Godot;
using System;

public partial class Soldier : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;

	public const float WanderDistance = 100.0f;
	public const float objectRange = 50.0f;

	 enum States {IDLE,WANDER,FOLLOW,SHOOT,GATHER,SEEK,GUARD};
	 enum WanderStates {IDLE,MOVING};
	 enum CommandStates{FOLLOW,MINE,GAURD,NONE};
	public enum WanderDirections{LEFT,RIGHT,BOTH};

	 static float initialPosition = 0;
	 float futurePosition = 0;
	 bool following = false;
	 bool inRange = false;
	 bool isWandering = false;
	 bool isGathering = false;
	 float direction = 0;
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
   initialPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
   GD.Print( this.GetPath());
}

public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		movement(velocity,delta);
		animations();
		
	}
	
	//a signal function that is triggered by a player sending a command to a soldier following the player
	//depending on the object the soldier will either mine or guard
	public void playerCommandObject(string pathID, Sprite2D Obj, Signals.COMMANDS command, float offset)
	{
		GD.Print("soldier commanded to an object");
		GD.Print("soldier state: " + current);
		if(current == States.FOLLOW && this.GetPath() == pathID)
		{
			obj = Obj;
			if(command == Signals.COMMANDS.MINING)
			{
				GD.Print("soldier is mining");
				current = States.GATHER;
			}
			else if(command == Signals.COMMANDS.GUARDING)
			{
				GD.Print("soldier is guarding");
				current = States.GUARD;
				initialPosition = Obj.GlobalPosition.X + offset;
				GD.Print("object position:" + Obj.GlobalPosition.X + offset);
			}
			
		}
	}
	public void playerCommandFollow()
	{
		GD.Print("soldier recieved command");
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		//turns off the wandering or gathering animations
		isGathering = false;
		
		if(inRange)
		{
			isWandering = false;
			if(current != States.FOLLOW)
			{
				current = States.FOLLOW;
				GD.Print("player has commanded soldier to follow");
				customSignals.EmitSignal(nameof(customSignals.followingPlayer),this.GetPath());
			}
			else
			{
				GD.Print("player has commanded soldier to wander");
				current = States.WANDER;
				initialPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
			}
			GD.Print("status: " + current);
		}
		
	}


	public void playerEntered(Node2D body)
	{
		var material = GetNode<AnimatedSprite2D>("soldierBody").Material;
		if(inRange)
		{
			inRange = false;
			(material as ShaderMaterial).SetShaderParameter("onoff", 0);
		}
		else
		{
			inRange = true;
			if(!following)
			(material as ShaderMaterial).SetShaderParameter("onoff", 1);
		}
	}
	 
	public void animations()
	{
		var body = GetNode<AnimatedSprite2D>("soldierBody");
		if(direction > 0)
		{
			body.FlipH = false;
		}
		else if(direction < 0)
		{
			body.FlipH = true;
		}

		if(direction != 0)
			body.Play("walk");
		else if(isGathering)
			body.Play("gather");
		else
			body.Play("idle");
	}

	public void movement(Vector2 velocity,double delta)
	{
		int[] distances = {25,50,75};
		int[] directionChoice = {-1,1};
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		//where states are checked on
		if(current == States.WANDER)
			wandering();
		else if(current == States.FOLLOW)
			follow();
		else if(current == States.GATHER)
			gather();
		else if(current == States.GUARD)
		{ // i think seek is the issue for guarding
			seek();
			
		}
			
		
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}

	public bool seek(float objRange = objectRange)
	{
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		bool withinRange = false;
		if( soldier.GlobalPosition.X < obj.GlobalPosition.X - objRange && soldier.GlobalPosition.X < obj.GlobalPosition.X + objRange)
		{
			direction = 2;
			withinRange = false;
		}
		else if(soldier.GlobalPosition.X > obj.GlobalPosition.X - objRange && soldier.GlobalPosition.X > obj.GlobalPosition.X + objRange)
		{
			direction = -2;
			withinRange = false;
		}
		else
			withinRange = true;

		return withinRange;
			 
	}

	public void gather()
	{
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		var timer = GetNode<Timer>("GatherTimer");
		if(seek())
		{
			direction = 0;
			if(!isGathering)
			{
				isGathering = true;
				timer.Start(3);
				GD.Print("gathering started!");
			}
			else
			{
				if(timer.IsStopped())
				{
					customSignals.EmitSignal(nameof(customSignals.resourceMined),obj);
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
		
		if(!inRange)
		{
			if(Global.playerInstance.GlobalPosition.X > soldier.GlobalPosition.X)
			 	direction = 2;
			 else
			 	direction = -2;
		}
		else
			direction = 0;
	}

	//its stuttering when choosing to move twice
	public void wandering(WanderDirections directions = WanderDirections.BOTH,float distance = WanderDistance)
	{
		
		var timer = GetNode<Timer>("wanderTimer");
		int[] distances = {25,50,75};
		int[] directionChoice = {-1,1};
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
		bool withinL = true;
		bool withinR = true;
		

		if(wanderCurrent == WanderStates.IDLE)
		{ 
			//GD.Print("i am idle");
			direction = 0;
			if(!isWandering)
			{
				timer.Start(GD.Randi() % 2 + 1);
				isWandering = true;
			}
			else if(timer.IsStopped())
			{
				//GD.Print("timer stopped");
				wanderCurrent = WanderStates.MOVING;
				isWandering = false;
			}
		}
		else if(wanderCurrent == WanderStates.MOVING)
		{
			//GD.Print("i am moving");
			if(!isWandering)
			{
					
				direction =  directionChoice[GD.Randi() % 2];
				tempDistance = distances[GD.Randi() % 3];
				// // checks if the soldier's next move is within allowed range on the left or right
				// if((tempDistance * direction) + currentPosition > initialPosition + distance)
				// 	withinR = false;

				// if((tempDistance * direction) + currentPosition < initialPosition + distance)
				// 	withinL = false;
				
				switch(directions)
				{
					case WanderDirections.LEFT:
						if(withinL == false || (tempDistance * direction) + currentPosition < initialPosition)
						{
							direction = - direction;
							GD.Print("direction change!");
						}
					break;
					case WanderDirections.RIGHT:
						if(withinR == false || (tempDistance * direction) + currentPosition > initialPosition)
							direction = - direction;
					break;
					case WanderDirections.BOTH:
						if(withinR == false || withinL == false)
							direction = - direction;
					break;
				}
				
				// // if((tempDistance * direction) + currentPosition  > initialPosition + (100 * direction))
				// // 	direction = - direction;
				//GD.Print("global position: " + this.GlobalPosition.X);
				futurePosition =  currentPosition + (tempDistance * direction) ;
				//GD.Print("current position: " + currentPosition + " future positon: " + futurePosition + " object position: " + initialPosition);
				//GD.Print("current position: " + currentPosition +" next position: " + futurePosition);
				isWandering = true;	

			}
			else
			{
				if((direction == -1 && currentPosition <= futurePosition) || 
				(direction == 1 && currentPosition >= futurePosition ))
				{	
					//GD.Print("going back to idle")
					isWandering = false;
					wanderCurrent = WanderStates.IDLE;
					
				}	
				
			}
		}
	
	}
	
}
