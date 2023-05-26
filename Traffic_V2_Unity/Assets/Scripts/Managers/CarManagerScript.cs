using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CarManagerScript : MonoBehaviour
{
    // References
    GameObject[] CarArray;

    private void Awake()
    {
        //Fill References
        CarArray = new GameObject[RunSettings.CarNum];
    }

    public void SpawnAllCars()
    {
        for (int i = 0; i < RunSettings.CarNum; i++)
        {

            SpawnOneCar(i);

        }
    }

    private void SpawnOneCar(int CarID)
    {

        // Create the game object
        CarArray[CarID] = GameObject.Instantiate(Resources.Load("Lane")) as GameObject;

        // Name
        CarArray[CarID].name = "Lane_" + CarID;

        // Set as child
        CarArray[CarID].transform.SetParent(gameObject.transform);

        //Call 'Spawn', passing in attributes
        //CarArray[CarID].GetComponent<PathCreation.Examples.>().placeCar(CarID, lane);
    }
}
