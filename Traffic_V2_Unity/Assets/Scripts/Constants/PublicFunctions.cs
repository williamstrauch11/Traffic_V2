using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PublicFunctions
{
    static public int AssignLanes(int CarID)
    {
        // Distributed evenly for now
        return CarID % RunSettings.LANENUM;
    }
}
