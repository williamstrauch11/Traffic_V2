using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class PublicFunctions
{
    static public int AssignLanes(int CarID)
    {
        // Distributed evenly for now
        return CarID % RunSettings.LANENUM;
    }

    static public float SpawnPosition(float _laneLength, int _carID)
    {
        float fraction = _laneLength / RunSettings.CARNUM;
        return fraction * _carID;
    }
}
