using Godot;
using System;

public partial class Global : Node
{
    //private PackedScene player = ResourceLoader.Load<PackedScene>("res://scenes/bullet.tscn");
    public static AnimatedSprite2D playerInstance  {get; set;}
}
