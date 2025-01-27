using Godot;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    public List<Vector2> BarricadeLocations = new List<Vector2>();
    public int test = 0;

    public override void _Ready()
    {
    }

    public void add_barricade(Vector2 data)
    {
        BarricadeLocations.Add(data);
        BarricadeLocations.Sort();
    }
}
