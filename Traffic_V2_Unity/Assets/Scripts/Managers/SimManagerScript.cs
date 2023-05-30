using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SimManagerScript : MonoBehaviour
{
    // References 
    CarManagerScript carManagerScript;
    LaneManagerScript laneManagerScript;

    // Startup
    private bool StartupFinished;

    private void Awake()
    {

        // Fill References
        carManagerScript = new CarManagerScript();
        laneManagerScript = new LaneManagerScript();

    }
    // Start is called before the first frame update
    private void Start()
    {

        StartCoroutine(Startup());

    }

    // Update is called once per frame
    private void Update()
    {
        if (StartupFinished)
        {
     
        }
    }

    IEnumerator Startup()
    {
        StartupFinished = false;

        // Spawn Lanes
        laneManagerScript.SpawnAllLanes();

        // Spawn Cars
        carManagerScript.SpawnAllCars();

        // Wait for next frame
        yield return new WaitForEndOfFrame();

        // Update Lanes to clear visual issues
        laneManagerScript.UpdateLanes();

        StartupFinished = true;

    }
}
