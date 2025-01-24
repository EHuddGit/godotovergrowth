using Godot;
using Godot.Collections;
using System;
using System.ComponentModel;

public partial class Signals : Node
{
    public enum COMMANDS {MINING = 1, GUARDING, WANDERING};
    [Signal]
    public delegate void playerCommandingEventHandler();
    [Signal]
    public delegate void playerCommandingObjectEventHandler(string pathID, Sprite2D obj,COMMANDS command, float xOffset);
    [Signal]
    public delegate void followingPlayerEventHandler(string pathID);
    [Signal]
    public delegate void objectNearbyEventHandler(Sprite2D nearbyObject,Array<bool> followerspots);
    [Signal]
    public delegate void resourceMinedEventHandler(Sprite2D resource);
    [Signal]
    public delegate void soldierMiningEventHandler(AnimatedSprite2D soldier);
    [Signal]
    public delegate void resourceModifyEventHandler(int amount,bool isAdd);

    [Signal]
    public delegate void enemyDamageEventHandler(string pathID);
    
}
