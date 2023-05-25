using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class pathCreatorTest : MonoBehaviour
{
    private int nodeNum = 6;
    private Vector2[] circleOfPoints;
    private float radius = 20f;


    // Start is called before the first frame update
    void Start()
    {
        circleOfPoints = new Vector2[nodeNum];
        radius = 20f;

        for (int i = 0; i < nodeNum; i++)
        {
            float angle = (2 * Mathf.PI) * (i / nodeNum);

            circleOfPoints[i].x = Mathf.Sin(angle) * radius;
            circleOfPoints[i].y = Mathf.Cos(angle) * radius;

        }


        GeneratePath(circleOfPoints, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    VertexPath GeneratePath(Vector2[] points, bool closedPath)
    {
        // Create a closed, 2D bezier path from the supplied points array
        // These points are treated as anchors, which the path will pass through
        // The control points for the path will be generated automatically
        BezierPath bezierPath = new BezierPath(points, closedPath, PathSpace.xy);
        // Then create a vertex path from the bezier path, to be used for movement etc
        return new VertexPath(bezierPath,transform);
    }
}
