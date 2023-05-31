using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CarManagerScript : MonoBehaviour
{

    // References
    [HideInInspector] public GameObject[] CarArray;
    [HideInInspector] public CarScript[] CarScriptArray;


    // Constructor 
    void Awake()
    {
        // References fill
        CarArray = new GameObject[RunSettings.CARNUM];
        CarScriptArray = new CarScript[RunSettings.CARNUM];
    }


    // Called from SimManagerScript upon startup
    public void SpawnAllCars()
    {
        // Spawn each car
        for (int i = 0; i < RunSettings.CARNUM; i++)
        {
            SpawnOneCar(i);

            // Add to CarScriptArary
            CarScriptArray[i] = CarArray[i].GetComponent<CarScript>();
        }
    }

    public void RunCars(float dt)
    {
        for (int i = 0; i < RunSettings.CARNUM; i++)
        {
            CarScriptArray[i].Run(dt);
        }
    }



    private void SpawnOneCar(int CarID)
    {

        // Create the game object
        CarArray[CarID] = GameObject.Instantiate(Resources.Load("Prefabs/Car")) as GameObject;

        // Name
        CarArray[CarID].name = "Car_" + CarID;

        // Set as child of LaneManager GameObject
        CarArray[CarID].transform.SetParent(this.gameObject.transform);

        // Call Spawn function in CarScript(Monobehavior)
        CarArray[CarID].GetComponent<CarScript>().SpawnCar(CarID, PublicFunctions.AssignLanes(CarID));

    }
}