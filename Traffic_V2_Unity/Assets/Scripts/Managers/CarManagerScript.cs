using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CarManagerScript
{

    // References
    readonly GameObject[] CarArray;
    readonly GameObject CarManager;


    // Constructor 
    public CarManagerScript()
    {
        // Fill References
        CarArray = new GameObject[RunSettings.CARNUM];
        CarManager = GameObject.Find("CarManager");

    }


    // Called from SimManagerScript upon startup
    public void SpawnAllCars()
    {
        // Spawn each lane
        for (int i = 0; i < RunSettings.CARNUM; i++)
        {
            SpawnOneLane(i);
        }
    }



    private void SpawnOneLane(int CarID)
    {

        // Create the game object
        CarArray[CarID] = GameObject.Instantiate(Resources.Load("Car")) as GameObject;

        // Name
        CarArray[CarID].name = "Car_" + CarID;

        // Set as child of LaneManager GameObject
        CarArray[CarID].transform.SetParent(CarManager.transform);

        // Call Spawn function in CarScript(Monobehavior)
        // CarArray[CarID].GetComponent<CarScript>().placeCar(CarID, lane);

    }
}