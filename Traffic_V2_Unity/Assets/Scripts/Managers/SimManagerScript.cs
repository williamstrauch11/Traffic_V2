using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SimManagerScript : MonoBehaviour
{
    // References 
    GameObject CarManager;
    LaneManagerScript LaneManager;

    // Startup
    private bool StartupFinished;

    private void Awake()
    {

        // Fill References
        CarManager = GameObject.Find("CarManager");
        LaneManager = new LaneManagerScript();

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
        LaneManager.SpawnAllLanes();

        // Spawn Cars
        // CarManager.GetComponent<CarManagerScript>().SpawnAllCars();

        // Wait for next frame
        yield return new WaitForEndOfFrame();

        // Update Lane Visuals
        LaneManager.UpdateLanes();

        StartupFinished = true;

    }
}
