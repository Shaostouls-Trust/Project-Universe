using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reference class with all materials available for easy reference in other classes without paramateriazation.
/// 
/// </summary>
public static class MaterialReferences
{
    //come off a pink. Not loaded.

    //Display Panel Materials
    public static Material display_Unlocked = Resources.Load("StatusDisplay_Unlocked", typeof(Material)) as Material;
    public static Material display_Locked = Resources.Load("StatusDisplay_Locked", typeof(Material)) as Material;
    public static Material display_Transit = Resources.Load("StatusDisplay_Transit", typeof(Material)) as Material;
    public static Material display_Stuck = Resources.Load("StatusDisplay_Stuck", typeof(Material)) as Material;
	
}
