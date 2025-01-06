using Godot;
using System;
using System.ComponentModel;

public partial class Signals : Node
{
    [Signal]
    public delegate void playerCommandingEventHandler();
    [Signal]
    public delegate void playerCommandingMineEventHandler(string pathID, Sprite2D mineable);
    [Signal]
    public delegate void followingPlayerEventHandler(string pathID);
    [Signal]
    public delegate void objectNearbyEventHandler(Sprite2D nearbyObject);
    [Signal]
    public delegate void resourceMinedEventHandler(Sprite2D resource);
    [Signal]
    public delegate void soldierMiningEventHandler(AnimatedSprite2D soldier);
    [Signal]
    public delegate void resourceModifyEventHandler(int amount,bool isAdd);
    
}
