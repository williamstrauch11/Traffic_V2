using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class LaneManagerScript : MonoBehaviour
{

    // References
    GameObject[] LaneArray;

    private void Awake()
    {
        // Fill References
        LaneArray = new GameObject[RunSettings.LaneNum];
    }

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

        // Set as child
        LaneArray[LaneID].transform.SetParent(gameObject.transform);

        StartCoroutine(LaneUpdater());
        
    }

    public void UpdateLanes()
    {
        // Update each lane to set visuals
        for (int i = 0; i < RunSettings.LaneNum; i++)
        {

            LaneArray[i].GetComponent<PathCreation.Examples.LaneUpdater>().UpdatePath();

        }
    }

    IEnumerator LaneUpdater()
    {
        yield return null;

        UpdateLanes();
    }
}
