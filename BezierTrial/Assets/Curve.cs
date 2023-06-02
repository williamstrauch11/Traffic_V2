using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class Curve : MonoBehaviour
{
    [Range(0.001f,0.1f)]
    public float increment; // 0.04 seems about good

    [Range(0.00001f, 0.1f)]
    public float Threshold; // 0.1 gives about 3-4 recursions, seems nice and stable. Max I would do is 0.01, which gives about 8 recursions. Lets do 0.02

    private float[] nodeArray;

    private int runNum;

    private void OnDrawGizmos()
    {
        nodeArray = new float[(int)(1 / increment)];

        Debug.Log(nodeArray.Length);

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
        float TrialDistance = Vector3.Distance(A, D) / 2; // This seems to be fine, all resulting values fall within the range
        float TrialResult = CurveCalc(TrialDistance, A, D, Direction_A, Direction_B, MagRatio);

        runNum = 0;

        float FinalDistance = CurveTrial(TrialDistance, A, D, Direction_A, Direction_B, MagRatio, TrialDistance/2, TrialResult);

        Vector3 B = A + (Direction_A * MagRatio * FinalDistance);
        Vector3 C = D + (Direction_B * FinalDistance);

        CurveDraw(A, B, C, D);

    }

    public float CurveTrial(float _trialDistance, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio, float _cutter, float _trialResult)
    {

        // Lower and Upper results
        float _lowerResult = CurveCalc(_trialDistance - _cutter, A, D, _direction_A, _direction_B, _magRatio);
        float _upperResult = CurveCalc(_trialDistance + _cutter, A, D, _direction_A, _direction_B, _magRatio);

        // Minimum calculation
        float _minResult; 
        float _updatedDistance;

        if (_lowerResult <= _trialResult && _lowerResult <= _upperResult)
        {
            _updatedDistance = _trialDistance - _cutter;
            _minResult = _lowerResult;
        }
        else if (_trialResult <= _lowerResult && _trialResult <= _upperResult)
        {
            _updatedDistance = _trialDistance;
            _minResult = _trialResult;
        }
        else
        {
            _updatedDistance = _trialDistance + _cutter;
            _minResult = _upperResult;
        }

        
        // Average difference calculation for recursion bounds
        float _averageDif = (Mathf.Abs(_trialResult - _lowerResult) + Mathf.Abs(_upperResult - _trialResult)) / 2;


        // Recursive decision tree
        if (runNum > 50)
        {
            Debug.Log("Errored out:  " + runNum);
            return _updatedDistance;
        }

        else if (_averageDif > Threshold)
        {
            runNum++;
            return CurveTrial(_updatedDistance, A, D, _direction_A, _direction_B, _magRatio, _cutter / 2, _minResult);
        }
        else
        {
            Debug.Log("Finished in:  " + runNum);
            return _updatedDistance;
        }
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

        int i = 0;
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
                nodeArray[i] = _distance;
                i++;
            }
            

        }

        return 1 / Mathf.Min(nodeArray);
        
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
