using Godot;
using System;

public partial class Soldier : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;

	 enum States {IDLE,WANDER,FOLLOW,SHOOT};
	 enum WanderStates {IDLE,MOVING};

	 float initialPosition = 0;
	 float futurePosition = 0;
	 bool following = false;
	 bool inRange = false;
	 bool isWandering = false;
	 float direction = 0;

	States current = States.WANDER;
	WanderStates wanderCurrent = WanderStates.MOVING;

public override void _Ready()
{
    //var area = GetNode<Area2D>("commandArea");
	//area.Connect("body_shape_entered",playerEntered);
    //area.BodyEntered +=  playerEntered;
	//timer.Timeout += OnTimerTimeout;
   
}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		statesChange();
		movement(velocity,delta);
		animations();
		
	}

	public void playerEntered(Node2D body)
	{
		if(inRange)
			inRange = false;
		else
			inRange = true;
		GD.Print("player nearby! flag is: " + inRange);
	}
	 public void statesChange()
	{
		if(Input.IsActionPressed("command") && inRange)
		{
			current = States.FOLLOW;
			direction = 0;
			
			if(!following)
				following = true;
		}
		else if(!following)
		{
			current = States.WANDER;
			initialPosition = GetNode<AnimatedSprite2D>("soldierBody").GlobalPosition.X;
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
		
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}
	public void follow()
	{
		// not working since im not grabing the instance of the player and is grabbing the class?
		//var player = GetNode<AnimatedSprite2D>("playerBody");
		PackedScene playerScene = ResourceLoader.Load<PackedScene>("res://scenes/player.tscn");
		var soldier = GetNode<AnimatedSprite2D>("soldierBody");
		var parent = GetParent();
		var idk = playerScene.Instantiate();
		GD.Print("this is the player" + (idk as Node2D).GlobalPosition.X);
		//var body = player.GetNode<AnimatedSprite2D>("playerBody");
		
		//playerScene.GetNode<AnimatedSprite2D>("playerBody");
		//player.Get
		//player.FlipH = true;
		//GD.Print(body.GlobalPosition.X);
		if(!inRange)
		{
			// if(player.GlobalPosition.X > soldier.GlobalPosition.X)
			// 	direction = 1;
			// else
			// 	direction = -1;
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
			direction = 0;
			if(!isWandering)
			{
				timer.Start(GD.Randi() % 2 + 1);
				isWandering = true;
			}
			else if(timer.IsStopped())
			{
				GD.Print("timer stopped");
				wanderCurrent = WanderStates.MOVING;
				isWandering = false;
			}
		}
		else if(wanderCurrent == WanderStates.MOVING)
		{
	
			if(!isWandering)
			{
					
				direction = directionChoice[GD.Randi() % 2];
				GD.Print(direction);
				tempDistance = distances[GD.Randi() % 3];
				// // if((tempDistance * direction) + currentPosition  > initialPosition + (100 * direction))
				// // 	direction = - direction;

				futurePosition = (tempDistance * direction) + currentPosition;
				GD.Print("current position: " + currentPosition +" next position: " + futurePosition);
				isWandering = true;	

			}
			else
			{
				if((direction == -1 && currentPosition <= futurePosition) || 
				(direction == 1 && currentPosition >= futurePosition ))
				{	
					isWandering = false;
					GD.Print("stopped moving");
					wanderCurrent = WanderStates.IDLE;
					
				}	
				
			}
		}
		 GD.Print("future position: " + futurePosition + " direction: " + direction);
		
		
	}
	
}
