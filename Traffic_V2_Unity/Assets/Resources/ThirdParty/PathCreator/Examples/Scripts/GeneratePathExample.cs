using UnityEngine;

namespace PathCreation.Examples {
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class GeneratePathExample : MonoBehaviour {

        private bool closedLoop = true;

        private int _nodeNum = 100;
        private float _radius = 10f;

        private Vector2[] waypoints;

        
        void Start () {

            waypoints = new Vector2[_nodeNum];

            for (int i = 0; i < _nodeNum; i++)
            {
                float _circleAngle = ((float)i / _nodeNum) * 2 * Mathf.PI;


                waypoints[i].x = _radius * Mathf.Cos(_circleAngle);
                waypoints[i].y = _radius * Mathf.Sin(_circleAngle);

         
            }

            if (waypoints.Length > 0) {

                Debug.Log(waypoints);
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath (waypoints, closedLoop, PathSpace.xyz);
                GetComponent<PathCreator> ().bezierPath = bezierPath;
            }
        }
    }
}