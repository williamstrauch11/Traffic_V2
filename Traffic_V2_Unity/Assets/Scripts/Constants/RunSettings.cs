﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RunSettings
{


    //Units of length are meters


    // Lane
    public static int LANENUM { get; } = 4;
    public static float LANE_WIDTH { get; } = (3.6576f / 2f); // 3.6576 equals 12 feet. PathCreator doubles width for some reason, so we divide by 2 here.


    //Car
    public static int CARNUM { get; } = 1;
    public static float COMPUTATION_DELAY = 0.1f;


    

}
