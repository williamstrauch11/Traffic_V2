using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Curve1 : MonoBehaviour
{
    [Range(0.001f,0.1f)]
    public float increment; // 0.04 seems about good

    [Range(0.00001f, 0.1f)]
    public float Threshold; // 0.1 gives about 3-4 recursions, seems nice and stable. Max I would do is 0.01, which gives about 8 recursions. Lets do 0.02

    private List<float> nodeList;

    private int runNum;

    private void OnDrawGizmos()
    {

        nodeList.Clear();
        increment = 0.01f;
        Threshold = 0.01f;
            
        Vector3 A = transform.GetChild(0).position;  // Anchor A
        Vector3 D = transform.GetChild(1).position; // Anchor B

        Vector3 Direction_1 = transform.GetChild(2).position; // Node of A (Direction of which is the heading, distance from A is velocity)
        Vector3 Direction_2 = transform.GetChild(3).position; // Node of B ("")


        float Mag_A = Vector3.Distance(A,Direction_1);
        float Mag_B = Vector3.Distance(D, Direction_2);

        float MagRatio = Mag_A / Mag_B;

        Vector3 Direction_A = Vector3.Normalize(Direction_1 - A);
        Vector3 Direction_B = Vector3.Normalize(Direction_2 - D);

        // Starting values. Distance is the node's distance from its anchor, which gets multiplied by MagRatio and direction
        // Result is the standard deviation for this distance

        float TrialDistance = Vector3.Distance(A, D) / 6; // This seems to be fine, all resulting values fall within the range

        runNum = 0;

        float FinalDistance = CurveTrial(0f, TrialDistance * 2, A, D, Direction_A, Direction_B, MagRatio);

        Vector3 B = A + (Direction_A * MagRatio * FinalDistance);
        Vector3 C = D + (Direction_B * FinalDistance);

        CurveDraw(A, B, C, D);

    }

    public float CurveTrial(float _min, float _max, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio)
    {

        float _range = Mathf.Abs(_max - _min);
        int _count = 20;

        if (_range < Threshold)
        {
            Debug.Log(runNum);
            return _min + (_range / 2);
        }

        float[,] _trialsArray = new float[_count, 2];


        int pos = 0;
        for (int i = 0; i < _count; i++)
        {
            nodeList.Clear();

            float _value = _min + i * (_range / _count);
            float _fValue = CurveCalc(_value, A, D, _direction_A, _direction_B, _magRatio);
            _trialsArray[i, 0] = _value;
            _trialsArray[i, 1] = _fValue;

            

            // Get min in pos
            if (_trialsArray[i, 1] < _trialsArray[pos, 1]) { pos = i; }
        }

        int _divisor = 8;

        float _newMin = _trialsArray[pos, 0] - (_range / _divisor);

        if (_newMin < 0) { _newMin = 0; }

        float _newMax = _trialsArray[pos, 0] + (_range / _divisor);

        runNum++;

        
        return CurveTrial(_newMin, _newMax, A, D, _direction_A, _direction_B, _magRatio);

    }

    // Set up curves to be tested
    public float CurveCalc(float _factor, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio)
    {

        Vector3 Trial_A = A + (_direction_A * _magRatio * _factor);
        Vector3 Trial_B = D + (_direction_B * _factor);

        return TestCurve(A, Trial_A, Trial_B, D);
    }

    // Test each curve
    public float TestCurve(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {

        Vector3 _oldPoint = new Vector3();
        Vector3 _point = new Vector3();

        
        for (float t = 0; t <= 1; t += increment)
        {
            _oldPoint = _point;

            _point = Mathf.Pow(1 - t, 3) * A +
                3 * Mathf.Pow(1 - t, 2) * t * B +
                3 * (1 - t) * Mathf.Pow(t, 2) * C +
                Mathf.Pow(t, 3) * D;



            if (t != 0)
            {
                float _distance = Vector3.Distance(_oldPoint, _point);
                nodeList.Add(_distance);
            }
            

        }

        return 1 / nodeList.Min();
        
    }

    // Draw the final curve
    public void CurveDraw(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(B, 0.1f);
        Gizmos.DrawWireSphere(C, 0.1f);

        Gizmos.color = Color.white;
        for (float t = 0; t <= 1; t += increment)
        {


            Vector3 _point = Mathf.Pow(1 - t, 3) * A +
                3 * Mathf.Pow(1 - t, 2) * t * B +
                3 * (1 - t) * Mathf.Pow(t, 2) * C +
                Mathf.Pow(t, 3) * D;

            Gizmos.DrawWireSphere(_point, 0.1f);

        }
    }
    

}
