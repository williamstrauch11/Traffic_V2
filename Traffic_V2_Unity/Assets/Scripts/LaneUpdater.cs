using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

namespace PathCreation.Examples
{

    public class LaneUpdater : MonoBehaviour
    {

        RoadMeshCreator meshTest;

        private void Awake()
        {
            meshTest = gameObject.GetComponent<RoadMeshCreator>();
        }


        public void UpdatePath()
        {
            meshTest.PathUpdatedTwo();
            Debug.Log("HI");
        }

    }
}