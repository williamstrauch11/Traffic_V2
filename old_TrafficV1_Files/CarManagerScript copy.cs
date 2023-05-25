using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;
using System;
using System.Numerics;

public class CarManagerScript : MonoBehaviour
{
    //Manually populated
    public GameObject LaneManager;

    //Create array of blank gameobjects for cars to be filled later
    public GameObject[] cars_object;

    //Create array of the car's scripts to be filled later
    public CarScript[] cars_scripts;
    FieldOfView[] fov_scripts;

    //Variables
    public int laneNumber;

    private bool running = false;

    //Awake
    private void Awake()
    {
        //Set lane number
        laneNumber = LaneManager.transform.childCount;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {

        //Gameobject array
        cars_object = new GameObject[constants.CarNum];

        //Let the lanes form before spawning cars
        yield return new WaitForSeconds(0.25f);

        Debug.Log(cars_object.Length);

        //spawn cars
        for (int i = 0; i < constants.CarNum; i++)
        { 
            //For now, we distribute cars on each lane evenly
            //int lane_given = i % laneNumber;
            int lane_given = 0;
           
            CarSpawn(i,lane_given);
        }

        //get scripts
        cars_scripts = GetComponentsInChildren<CarScript>();
        fov_scripts = GetComponentsInChildren<FieldOfView>();

        running = true;

    }

    private void CarSpawn(int CarID, int lane)
    {

        //Create the game object
        cars_object[CarID] = GameObject.Instantiate(Resources.Load("Car")) as GameObject;

        //Name
        cars_object[CarID].name = "Car_" + CarID;

        //Set as child
        cars_object[CarID].transform.SetParent(gameObject.transform);

        //Call 'Spawn', passing in attributes
        cars_object[CarID].GetComponent<CarScript>().placeCar(CarID, lane);
    }


    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            //update all the cars
            for (int i = 0; i < constants.CarNum; i++)
            {
                //Call the run fuction. Pass in the car_script of the car in front
                cars_scripts[i].Run(fov_scripts[i].lead_index_final, Time.deltaTime);

            }
        }
        
    }
}
