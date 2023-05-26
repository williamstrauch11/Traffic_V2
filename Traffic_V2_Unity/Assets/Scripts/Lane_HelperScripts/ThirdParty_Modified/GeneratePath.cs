using UnityEngine;

namespace PathCreation.Examples {

    // Example of creating a path at runtime from a set of points.
    [RequireComponent(typeof(PathCreator))]
    public class GeneratePath : MonoBehaviour {



        private bool closedLoop = true;

        [SerializeField] private int NodeNum;
        [SerializeField] private float Radius;

        private Vector2[] waypoints;

        public void CreateLane(int _nodeNum, float _radius) {

            NodeNum = _nodeNum;
            Radius = _radius;

            waypoints = new Vector2[NodeNum];

            for (int i = 0; i < NodeNum; i++)
            {
                float _circleAngle = ((float)i / NodeNum) * 2 * Mathf.PI;

                waypoints[i].x = Radius * Mathf.Cos(_circleAngle);
                waypoints[i].y = Radius * Mathf.Sin(_circleAngle);
         
            }

            if (waypoints.Length > 0) {

                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath (waypoints, closedLoop, PathSpace.xyz);
                GetComponent<PathCreator> ().bezierPath = bezierPath;
            }
        }
    }
}