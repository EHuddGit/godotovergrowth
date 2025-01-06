using Godot;
using System;

public partial class Soldier : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;

	 enum States {IDLE,WANDER,FOLLOW,SHOOT,GATHER};
	 enum WanderStates {IDLE,MOVING};
	 enum CommandStates{FOLLOW,MINE,NONE};

	 float initialPosition = 0;
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

	Sprite2D resource;
	


public override void _Ready()
{
   customSignals = GetNode<Signals>("/root/Signals");
   customSignals.playerCommanding += playerCommandFollow;
   customSignals.playerCommandingMine += playerCommandMine;
   GD.Print( this.GetPath());
}

public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		movement(velocity,delta);
		animations();
		
	}
	
	public void playerCommandMine(string pathID, Sprite2D mineable)
	{
		GD.Print("soldier commanded to mine");
		GD.Print("soldier state: " + current);
		if(current == States.FOLLOW && this.GetPath() == pathID)
		{
			GD.Print("soldier is mining");
			resource = mineable;
			current = States.GATHER;
		}
	}
	public void playerCommandFollow()
	{
		GD.Print("soldier recieved command");
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		//turns off the wandering or gathering animations
		//isGathering = false;
		
	
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

		if(current == States.WANDER)
			wandering();
		else if(current == States.FOLLOW)
			follow();
		else if(current == States.GATHER)
			gather();
		
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}

	public void gather()
	{
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		var timer = GetNode<Timer>("GatherTimer");
		if( soldier.GlobalPosition.X < resource.GlobalPosition.X - 50 && soldier.GlobalPosition.X < resource.GlobalPosition.X + 50)
			 	direction = 2;
		else if(soldier.GlobalPosition.X > resource.GlobalPosition.X - 50 && soldier.GlobalPosition.X > resource.GlobalPosition.X + 50)
			 	direction = -2;
		else
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
					customSignals.EmitSignal(nameof(customSignals.resourceMined),resource);
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
	public void wandering()
	{
		
		var timer = GetNode<Timer>("wanderTimer");
		int[] distances = {25,50,75};
		int[] directionChoice = {-1,1};
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
		

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
					
				direction = directionChoice[GD.Randi() % 2];
				tempDistance = distances[GD.Randi() % 3];
				// supposed to keep them within a 100 pixel distance of where they begin wandering
				// // if((tempDistance * direction) + currentPosition  > initialPosition + (100 * direction))
				// // 	direction = - direction;

				futurePosition = (tempDistance * direction) + currentPosition;
				//GD.Print("current position: " + currentPosition +" next position: " + futurePosition);
				isWandering = true;	

			}
			else
			{
				if((direction == -1 && currentPosition <= futurePosition) || 
				(direction == 1 && currentPosition >= futurePosition ))
				{	
					isWandering = false;
					wanderCurrent = WanderStates.IDLE;
					
				}	
				
			}
		}
	
	}
	
}
