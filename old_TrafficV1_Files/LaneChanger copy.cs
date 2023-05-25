using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Timeline.AnimationPlayableAsset;

public class LaneChanger : MonoBehaviour
{
    /*
    README
    Structure of this file given largely by a video on Bezier Curves:
    https://www.youtube.com/watch?v=11ofnLOE8pw&t=658s&ab_channel=AlexanderZotov
    Method to follow a quadratic Bezier curve using the derivative and three vectors given in this forum post:
    https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve/27138#27138?newreg=aab63b95e31c4f8abc8c5c6727e649b7
    */

    //Store reference to the Lane
    [SerializeField] private GameObject Lane;

    //Declarations
    private int routeToGo;
    private Vector3 carPosition;
    public Vector3 heading;
    private bool betweenRoutes;
    private bool loop;
    private int runs;
    private int fl_override;
    private int pointNum;
    private bool firstRun;

    public bool reset;

    //Carryover route variables
    private float L_a;
    private float L_b;

    private Vector3 v1;
    private Vector3 v2;
    private Vector3 v3;

    private Vector3 A;
    private Vector3 B;
    private Vector3 C;
    private Vector3 D;

    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;

    private float delta_t;

    private float t0;
    private float L;

    // Start is called before the first frame update
    void Start()
    {
        reset = true;

        betweenRoutes = true;

        //Set start
        routeToGo = 0;

        //First route starts at 0
        L_b = 0;

        fl_override = 0;

        pointNum = constants.nodeNum;

        //Set number of runs based on if the lane loops or not
        runs = pointNum - 2;
    }

    public Vector3 GetPosition(Vector3 carTransform, float L_given)
    {
        if (!reset)
            reset = true;

        L = L_given;
        carPosition = carTransform;

        //Check if you need the first and last override. At the first and last point, Bezier curve goes to node, and not to the midpoint
        if (betweenRoutes)
        {

            if (routeToGo == 0)
            {
                fl_override = 1;
            }
            else if (routeToGo == (runs - 1))
            {
                fl_override = 2;
            }

            SetNewRoute(routeToGo, (routeToGo + 1), (routeToGo + 2), fl_override);

            fl_override = 0;
        }

        PositionUpdate();

        return carPosition;

    }

    private Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t)
    {
        return A + (B - A) * t;
    }

    private float T_Update(float L, float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float t_update = t + L / Vector3.Magnitude((Mathf.Pow(t, 2) * v1) + (t * v2 + v3));

        return t_update;
    }

    private Vector3 Bezier_Heading(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        //Find direction of movement given t (0 < t < 1), and the three vectors for the quadratic curve calculated above. This is the derivative at a point (t) on the curve.
        Vector3 DmDt = (Mathf.Pow(t, 2) * v1) + (t * v2) + v3;

        //Heading vector
        return Vector3.Normalize(DmDt);

    }

    private void SetNewRoute(int point_a, int point_b, int point_c, int first_last_override)
    {
        //Don't start coroutine again, until we finish with the route
        betweenRoutes = false;


        //Get the three nodes
        p0 = gameObject.GetComponent<CarScript>().nodeArray[point_a];
        p1 = gameObject.GetComponent<CarScript>().nodeArray[point_b];
        p2 = gameObject.GetComponent<CarScript>().nodeArray[point_c];

        //Get the gravity of the middle node
        float grav = 0.8f;

        //Set bezier curve points. A is halfway between p0 and p1, D is halfway between p1 and p2. B and D are also on these lines, but adjusted based on the gravity.
        //When gravity = 1, B = C = p1
        A = LerpWithoutClamp(p0, p1, 0.5f);
        B = LerpWithoutClamp(p0, p1, grav);
        C = LerpWithoutClamp(p2, p1, grav);
        D = LerpWithoutClamp(p1, p2, 0.5f);

        //If we have passed an override, set point A or D
        if (first_last_override != 0)
        {
            if (first_last_override == 1)
            {
                A = p0;
            }
            else if (first_last_override == 2)
            {
                D = p2;
            }
        }

        //Compute three vectors from the derivative of the bezier curve
        v1 = (-3 * A) + (9 * B) - (9 * C) + (3 * D);
        v2 = (6 * A) - (12 * B) + (6 * C);
        v3 = (-3 * A) + (3 * B);


        //initialize t0, using the straggling L_b left from the last route.
        t0 = T_Update(L_b, 0f, v1, v2, v3);

    }

    void PositionUpdate()
    {
        float lag_threshold = 0.005f;
        //If we are experiencing a lag spike, iterate over a number of things

        //Add one, for min case
        int lag_runs = (int)Mathf.Round(delta_t / lag_threshold);

        if (lag_runs < 1)
            lag_runs = 1;

        float L_total = L;

        L /= lag_runs;

        for (int i = 0; i < lag_runs; i++)
        {
            //Update t0, based on the length of the movement and the bezier curve
            t0 = T_Update(L, t0, v1, v2, v3);

            //Update new position pointing direction 
            heading = Bezier_Heading(t0, v1, v2, v3);

            //Update carposition
            carPosition += heading * L;
        }

        //check if we are off of current route
        if (t0 >= 1)
        {
            //If yes, split up distance traveled (L) into distance traveled to point D, and distance to travel in next route
            L_a = Vector3.Distance(transform.position, D);
            L_b = L_total - L_a;
            
            //Place car at D and travel distance L_b in direction tangent
            Vector3 tangent = Vector3.Normalize(D - p1);

            carPosition = D + (tangent * L_b);

            //When this route is finished, incriment routeToGo
            routeToGo++;

            //If we are at end, loop
            if (routeToGo > runs - 1)
            {

                routeToGo = 0;
                reset = false;
            }
                

            //Allow for next route to start
            betweenRoutes = true;
        }
    }
}