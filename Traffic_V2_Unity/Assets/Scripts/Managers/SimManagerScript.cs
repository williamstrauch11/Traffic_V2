using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SimManagerScript : MonoBehaviour
{
    // References 
    GameObject CarManager;
    GameObject LaneManager;

    public event EventHandler OnSpawnFinished;

    private void Awake()
    {

        // Fill References
        CarManager = GameObject.Find("CarManager");
        LaneManager = GameObject.Find("LaneManager");

    }
    // Start is called before the first frame update
    private void Start()
    {
        // Spawn Lanes
        LaneManager.GetComponent<LaneManagerScript>().SpawnAllLanes();

        // Spawn Cars
        // CarManager.GetComponent<CarManagerScript>().SpawnAllCars();

        OnSpawnFinished?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

}
