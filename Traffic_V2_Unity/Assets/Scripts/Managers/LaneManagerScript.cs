using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class LaneManagerScript
{

    // References
    readonly GameObject[] LaneArray;
    readonly GameObject LaneManager;

    readonly LaneFieldManager[] laneFieldManagers;


    // Constructor 
    public LaneManagerScript()
    {
        // Fill References
        LaneArray = new GameObject[RunSettings.LaneNum];
        LaneManager = GameObject.Find("LaneManager");

        laneFieldManagers = new LaneFieldManager[RunSettings.LaneNum];
    }


    // Called from SimManagerScript upon startup
    public void SpawnAllLanes()
    {
        // Spawn each lane
        for (int i = 0; i < RunSettings.LaneNum; i++)
        {
            SpawnOneLane(i);
        }
    }



    private void SpawnOneLane(int LaneID)
    {

        // Create the game object
        LaneArray[LaneID] = GameObject.Instantiate(Resources.Load("Path")) as GameObject;

        // Name
        LaneArray[LaneID].name = "Lane_" + LaneID;

        // Set as child of LaneManager GameObject
        LaneArray[LaneID].transform.SetParent(LaneManager.transform);

        // Fill lane settings
        laneFieldManagers[LaneID] = new LaneFieldManager(LaneID);

        // Generate Path
        LaneArray[LaneID].GetComponent<PathCreation.Examples.GeneratePath>().CreateLane(laneFieldManagers[LaneID].NodeNum, laneFieldManagers[LaneID].Radius);

    }

    // Called from SimManagerScript after a frame
    public void UpdateLanes()
    {
        // Update each lane to set visuals
        for (int i = 0; i < RunSettings.LaneNum; i++)
        {
            // Update visual values
            LaneArray[i].GetComponent<PathCreation.Examples.RoadMeshCreator>().ParameterUpdate(laneFieldManagers[i].RoadWidth, laneFieldManagers[i].TextureTiling);

            // Update visuals
            LaneArray[i].GetComponent<PathCreation.Examples.RoadMeshCreator>().PathUpdatedForced();

        }
    }
}
