using Godot;
using System;

public partial class Signals : Node
{
    [Signal]
    public delegate void playerCommandingEventHandler();
    [Signal]
    public delegate void playerCommandingMineEventHandler(Sprite2D crystal);
    [Signal]
    public delegate void followingPlayerEventHandler(AnimatedSprite2D follower);
    [Signal]
    public delegate void objectNearbyEventHandler(Sprite2D nearbyObject);
    [Signal]
    public delegate void resourceMinedEventHandler(Sprite2D resource);
    [Signal]
    public delegate void soldierMiningEventHandler(AnimatedSprite2D soldier);
    
}
