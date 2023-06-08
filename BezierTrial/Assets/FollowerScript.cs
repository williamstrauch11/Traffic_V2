using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using System.Linq;
using static UnityEditor.PlayerSettings;

public class FollowerScript : MonoBehaviour
{
    public Curve1 curve1;

    public List<float> turnList = new();
    public List<float> xList = new();

    public Vector3 v1;
    public Vector3 v2;
    public Vector3 v3;

    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector3 D;

    public float PathTimer;

    public Vector3 newHeading;

    public Vector3 Position;
    public Vector3 Heading;

    public float AngleDif;

    // Start is called before the first frame update
    void Start()
    {
        curve1 = GameObject.FindGameObjectWithTag("curve").GetComponent<Curve1>();

        A = curve1.Node1;
        B = curve1.Node2;
        C = curve1.Node3;
        D = curve1.Node4;

        PathTimer = 0f;

        Calc();

        TeleportToStart();


    }

    // Update is called once per frame
    void Update()
    {
        if (PathTimer >= 1f)
        {
            float maxTurn = turnList.Max();

            Debug.Log("real  " + maxTurn);

            TeleportToStart();

            PathTimer = 0f;

            float _searchDivisor = 0.01f;

            float[,] _trialsArray = new float[(int)(1/_searchDivisor) + 1, 2];

            int j = 0;
            int pos = 0;

            for (float i = 0; i < 1; i += _searchDivisor)
            {
             

                turnList.Add(CurveAt(i, v1, v2, v3));

                float _xValue = i;
                float _yValue = CurveAt(i, v1, v2, v3);

                _trialsArray[j, 0] = _xValue;
                _trialsArray[j, 1] = _yValue;


                Debug.Log(_yValue);
                if (_trialsArray[j, 1] > _trialsArray[pos, 1]) { pos = j; }

                j++;


            }


            Debug.Log("x  " + _trialsArray[pos, 0]);
            Debug.Log("y  " + _trialsArray[pos, 1]);
            


        }


        float L = (8f * Time.deltaTime) + (0f * Time.deltaTime * Time.deltaTime / 2);

        MoveAlongPath(Time.deltaTime, L, v1, v2, v3);

        UpdatePosition(Position);
        UpdateRotation(newHeading, L);
    }

    private void UpdatePosition(Vector3 _position)
    {
        transform.position = _position;
    }

    private void UpdateRotation(Vector3 _heading, float L)
    {
        if (PathTimer > 0.01 && PathTimer < 9.99)
        {
            AngleDif = Vector3.Angle(Heading, newHeading) / L;

            turnList.Add(AngleDif);


            
        }
        

        // Debug.Log(AngleDif);

        float rot_z = Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

        Heading = newHeading;

    }

    public void TeleportToStart()
    {
        Position = A;
        

    }
    public void Calc()
    {

        // Compute three vectors from the derivative of the bezier curve
        v1 = (-3 * A) + (9 * B) - (9 * C) + (3 * D);
        v2 = (6 * A) - (12 * B) + (6 * C);
        v3 = (-3 * A) + (3 * B);
    }

    private void MoveAlongPath(float dt, float L, Vector3 _v1, Vector3 _v2, Vector3 _v3)
    {

        float lag_threshold = 0.005f;
        //If we are experiencing a lag spike, iterate over a number of things

        //Add one, for min case
        int lag_runs = (int)Mathf.Round(dt / lag_threshold);

        if (lag_runs < 1)
            lag_runs = 1;

        L /= lag_runs;

        for (int i = 0; i < lag_runs; i++)
        {

            //Update t0, based on the length of the movement and the bezier curve
            PathTimer = T_Update(L, PathTimer, _v1, _v2, _v3);

            //Update new position pointing direction 
            newHeading = Bezier_Heading(PathTimer, _v1, _v2, _v3);

            //Update carposition
            Position += newHeading * L;

        }

    }

    public float T_Update(float L, float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // float t_update = t + L / Vector3.Magnitude((Mathf.Pow(t, 2) * v1) + (t * v2 + v3));
        float t_update = t + L / Vector3.Magnitude(Mathf.Pow(t, 2) * v1 + t * v2 + v3);

        return t_update;
    }

    public Vector3 Bezier_Heading(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        //Find direction of movement given t (0 < t < 1), and the three vectors for the quadratic curve calculated above. This is the derivative at a point (t) on the curve.
        Vector3 DmDt = (Mathf.Pow(t, 2) * v1) + (t * v2) + v3;

        //Heading vector
        return Vector3.Normalize(DmDt);

    }

    public float CurveAt(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 d1 = FirstDerivative(t, v1, v2, v3);
        Vector3 d2 = SecondDerivative(t, v1, v2, v3);

        return Curvature(d1, d2);
    }


    public Vector3 FirstDerivative(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return (Mathf.Pow(t, 2) * v1) + (t * v2) + v3;
    }

    public Vector3 SecondDerivative(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return ((2 * t * v1) + v2);
    }

    public float Curvature(Vector3 d1, Vector3 d2)
    {
        return Vector3.Magnitude(Vector3.Cross(d1, d2)) / (Mathf.Pow(Vector3.Magnitude(d1), 3));
    }
}
