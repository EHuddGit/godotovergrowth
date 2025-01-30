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
    private List<float> BarricadeLocations = new List<float>();
    private List <bool> validBarricades = new List<bool>();
    public int test = 0;
    private int IDs = -1;

    public int getID()
    {
        IDs++;
        return IDs;
    }

    public void addBarricade(float location)
    {
       
        BarricadeLocations.Add(location);
        BarricadeLocations.Sort();
        validBarricades.Add(true);
    }

    public void updateBarricade(float location)
    {
        for(int index = 0; index < BarricadeLocations.Count; index++)
        {
            if(BarricadeLocations[index] == location)
            {
                validBarricades[index] = ( validBarricades[index] == false) ? true : false;
                break;
            }
        }
    }
    public float nearestFallbackBarricade(float location)
    {
        float nearest = 0;
        for(int index = 0; index < BarricadeLocations.Count; index++)
        {
            if(location > 0 && BarricadeLocations[index] < location && validBarricades[index] == true)
            {
                if(nearest != 0 && (location - BarricadeLocations[index]) < nearest || nearest == 0)
                {
                    nearest = BarricadeLocations[index];
                }
            }
            else if(location < 0 && BarricadeLocations[index] > location && validBarricades[index] == true)
            {
                if(nearest != 0 && (location - BarricadeLocations[index]) < nearest || nearest == 0)
                {
                    nearest = BarricadeLocations[index];
                }
            }
        }
       
       if(nearest == 0)
        GD.Print("couldnt find barricade, going to 0");
       return nearest;
    }

    public List<float> getBarricades()
    {
        return BarricadeLocations;
    }
}
