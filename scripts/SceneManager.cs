using Godot;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    struct barricadeData
    {
        public float location;
        public bool validBarricade;
    }
    private List<Barricade> Barricades = new List<Barricade>();
    public int test = 0;
    private int IDs = -1;

    public int getID()
    {
        IDs++;
        return IDs;
    }
    


/*****      Barricade functions     ****************************************************/
    public void addBarricade(Barricade data)
    {
        Barricades.Add(data);
       // Barricades.Sort();
    }

    public Barricade getBarricade(int id)
    {
        for(int index = 0; index < Barricades.Count; index++)
        {
            if(id == Barricades[index].ID)
            {
                return Barricades[index];
            }
        }

        GD.Print("could not find barricade");
        return default;
    }
    public Barricade nearestFallbackBarricade(float location)
    {
        Barricade nearest = new Barricade();
        GD.Print("this should be zero: " + nearest.GlobalPosition.X);
        for(int index = 0; index < Barricades.Count; index++)
        {
            if(location > 0 && Barricades[index].GlobalPosition.X < location && Barricades[index].health > 0)
            {
                if(nearest.GlobalPosition.X != 0 && (location - Barricades[index].GlobalPosition.X) < nearest.GlobalPosition.X || nearest.GlobalPosition.X == 0)
                {
                    nearest = Barricades[index];
                }
            }
            else if(location < 0 && Barricades[index].GlobalPosition.X > location && Barricades[index].health > 0)
            {
                if(nearest.GlobalPosition.X != 0 && (location - Barricades[index].GlobalPosition.X) < nearest.GlobalPosition.X || nearest.GlobalPosition.X == 0)
                {
                    nearest = Barricades[index];
                }
            }
        }
       
       if(nearest.GlobalPosition.X == 0)
        GD.Print("couldnt find barricade, going to 0");
       return nearest;
    }

    public List<Barricade> getBarricades()
    {
        return Barricades;
    }
}
