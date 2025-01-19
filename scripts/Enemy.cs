using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	public const float Speed = 50.0f;
	public const float JumpVelocity = -400.0f;

	public const float BarricadeAttackRange = 30.0f;
	public const float PlayerAttackRange = 20.0f;
	public const float SoldierAttackRange = 20.0f;

	enum States {IDLE,WANDER,FOLLOW,ATTACK,SEEK,};
	enum WanderStates {IDLE,MOVING};
	public enum WanderDirections{LEFT,RIGHT,BOTH};
	enum ATTACKTARGETS 	{BARRICADE,SOLDIER,PLAYER};
	States current = States.WANDER;
	WanderStates wanderCurrent = WanderStates.MOVING;
	ATTACKTARGETS attackObjName;
	float direction = 0;
	float futurePosition = 0;
	bool isWandering = false;
	bool enemyDetected = false;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		enemyRayCast();
		if(enemyDetected && current != States.FOLLOW && current != States.ATTACK)
			current = States.FOLLOW;

			
		movement(velocity,delta);
		animations();
	}

	public void animations()
	{
		var body = GetNode<AnimatedSprite2D>("enemyBody");
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
		else if(current == States.ATTACK)
			body.Play("attack1");
		else
			body.Play("idle");
	}

	public void enemyRayCast()
	{
		var raycast = GetNode<RayCast2D>("objectDetecter");
		var soldier = GetNode<AnimatedSprite2D>("enemyBody");

		if(soldier.FlipH && raycast.RotationDegrees != 180)
		{
		//	GD.Print("zero degrees");
				raycast.RotationDegrees = 180;
		}
		else if(!soldier.FlipH && raycast.RotationDegrees != 0)
		{
				raycast.RotationDegrees = 0;
		//		GD.Print("180 degrees");
		}
		//GD.Print(raycast.RotationDegrees);

		if(raycast.IsColliding())
		{
			enemyDetected = true;
			GD.Print("collison detected!");
			var collided = (Node)raycast.GetCollider();
			GD.Print("collided with: " + collided.GetPath());

			if(collided.GetPath().ToString().Contains("barricade"))
				attackObjName = ATTACKTARGETS.BARRICADE;
			else if(collided.GetPath().ToString().Contains("soldier"))
				attackObjName = ATTACKTARGETS.SOLDIER;
			else if(collided.GetPath().ToString().Contains("player"))
				attackObjName = ATTACKTARGETS.PLAYER;
				

		}
		else if(enemyDetected)
			enemyDetected = false;
	}

	public void movement(Vector2 velocity,double delta)
	{
		int[] distances = {25,50,75};
		int[] directionChoice = {-1,1};
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("enemyBody").GlobalPosition.X;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		//where states are checked on
		if(current == States.WANDER)
			wandering();
		else if(current == States.FOLLOW)
			follow();
		else if(current == States.ATTACK)
			attack();
		
			
		
		if (direction != 0)
			velocity.X = direction * Speed;
		else
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);

		Velocity = velocity;
		MoveAndSlide();
	}

	public void attack()
	{
		direction = 0;
		var raycast = GetNode<RayCast2D>("objectDetecter");
		float currentPosition = GetNode<AnimatedSprite2D>("enemyBody").GlobalPosition.X;
		var vect = raycast.GetCollisionPoint();
		float difference = currentPosition - vect.X;
		if(!raycast.IsColliding())
		{
			current = States.WANDER;
			wanderCurrent = WanderStates.IDLE;
		}
		if(difference > 0 && difference > 50)
			current = States.FOLLOW;
		else if(difference < 0 && difference < -50)
			current = States.FOLLOW;
	}

	public void follow(float attackDistance = 40)
	{
		var raycast = GetNode<RayCast2D>("objectDetecter");
		float currentPosition = GetNode<AnimatedSprite2D>("enemyBody").GlobalPosition.X;
		Vector2 vect = raycast.GetCollisionPoint();
		float followedirection = vect.X - currentPosition;

		if(currentPosition < 0)
		{
			followedirection = - followedirection;
			//attackDistance = - attackDistance;
		}
		GD.Print("followed direction: " + followedirection);
		//GD.Print("current position: " + currentPosition + "  > target stopping point: " + (vect.X + attackDistance));

		if(currentPosition <= (vect.X - attackDistance)) // && followedirection > 0)
		 	direction = 2;
		else if(currentPosition >= (vect.X + attackDistance))// && followedirection < 0)
			direction = -2;
		else
		{
			direction = 0;
			current = States.ATTACK;
		}
			
		// if( currentPosition < vect.X - attackDistance && currentPosition < vect.X + attackDistance)
		// 	direction = 2;

		// else if(currentPosition > vect.X - attackDistance && currentPosition > vect.X + attackDistance)
		// 	direction = -2;
		// else
		// 	current = States.ATTACK;
	}

	public void wandering()
	{
		var timer = GetNode<Timer>("wanderTimer");
		timer.OneShot = true;
		int[] distances = {25,50,75};
		int[] directionChoice = {-1,1};
		int tempDistance = 0;
		float currentPosition = GetNode<AnimatedSprite2D>("enemyBody").GlobalPosition.X;
		if(wanderCurrent == WanderStates.IDLE)
		{ 
			//GD.Print("i am idle");
			direction = 0;
			//GD.Print("time left:" + timer.TimeLeft);
			if(!isWandering)
			{
				timer.Start(GD.Randi() % 2 + 1);
				isWandering = true;
	//			GD.Print("made it to timer start");
			}
			else if(timer.IsStopped())
			{
	//			GD.Print("timer stopped");
				wanderCurrent = WanderStates.MOVING;
				isWandering = false;
			}
		}
		else if(wanderCurrent == WanderStates.MOVING)
		{
			if(!isWandering)
			{ //this picks a random distance and direction to go to and updates on the future position to head to
				direction =  directionChoice[GD.Randi() % 2];
				tempDistance = distances[GD.Randi() % 3];

				futurePosition =  currentPosition + (tempDistance * direction);
				isWandering = true;	
			}
			else
			{ //this is basically say if you reached your destination go and idle for abit
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
