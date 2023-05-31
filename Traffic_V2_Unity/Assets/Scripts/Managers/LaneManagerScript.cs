using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class LaneManagerScript : MonoBehaviour
{

    // References
    [HideInInspector] public GameObject[] LaneArray; // GameObject array of the lanes
    [HideInInspector] public float[] LaneLengths; // Float array of the lane lengths
    [HideInInspector] public PathCreator[] LaneScriptArray; // Array of PathCreator scripts

    [HideInInspector] public LaneFieldManager[] laneFieldManagers; // Array of LaneFieldManager C# scripts


    // Constructor 
    void Awake()
    {
        // Fill References
        LaneArray = new GameObject[RunSettings.LANENUM];
        LaneLengths = new float[RunSettings.LANENUM];
        LaneScriptArray = new PathCreator[RunSettings.LANENUM];

        laneFieldManagers = new LaneFieldManager[RunSettings.LANENUM];

        // Set lanes to z = -1, so that cars can be on 0
        SetPosition(0f, 0f, -1f);
    }


    // Called from SimManagerScript upon startup
    public void SpawnAllLanes()
    {
        // Spawn each lane
        for (int i = 0; i < RunSettings.LANENUM; i++)
        {
            SpawnOneLane(i);
        }
    }



    private void SpawnOneLane(int LaneID)
    {

        // Create the game object
        LaneArray[LaneID] = GameObject.Instantiate(Resources.Load("Prefabs/Path")) as GameObject;

        // Name
        LaneArray[LaneID].name = "Lane_" + LaneID;

        // Set as child of LaneManager GameObject
        LaneArray[LaneID].transform.SetParent(this.gameObject.transform);

        // Fill lane settings
        laneFieldManagers[LaneID] = new LaneFieldManager(LaneID);

        // Generate Path
        LaneArray[LaneID].GetComponent<PathCreation.Examples.GeneratePath>().CreateLane(laneFieldManagers[LaneID].NodeNum, laneFieldManagers[LaneID].Radius);

    }

    // Called from SimManagerScript after a frame
    public void UpdateLanes()
    {
        // Update each lane
        for (int i = 0; i < RunSettings.LANENUM; i++)
        {
            // Update visual values
            LaneArray[i].GetComponent<PathCreation.Examples.RoadMeshCreator>().ParameterUpdate(laneFieldManagers[i].RoadWidth, laneFieldManagers[i].TextureTiling);

            // Update visuals
            LaneArray[i].GetComponent<PathCreation.Examples.RoadMeshCreator>().PathUpdatedForced();

        }

        UpdateLaneInfo();
    }


    private void UpdateLaneInfo()
    {
        // Update each lane
        for (int i = 0; i < RunSettings.LANENUM; i++)
        {
            // Update script array
            LaneScriptArray[i] = LaneArray[i].GetComponent<PathCreator>();

            // Update length array
            LaneLengths[i] = LaneScriptArray[i].path.length;

        }
    }

    private void SetPosition(float x, float y, float z)
    {
        Vector3 temp = new Vector3(x, y, z);
        transform.position += temp;

    }
}
