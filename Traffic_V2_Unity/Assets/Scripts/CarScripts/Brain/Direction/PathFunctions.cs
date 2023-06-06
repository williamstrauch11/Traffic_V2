using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFunctions
{
    // Setup
    private LaneManagerScript laneManagerScript;

    public int _runNum_d2l { get; private set; } // runnum for distance to lane
    public int _runNum_rpc { get; private set; } // runnum for recursive path calculator

    // Recursive Path Calculator
    public List<float> nodeList = new();

    public PathFunctions()
    {

        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();
        

    }


    // ------------------------------------------------------------------------------------------------------------
    // LANE CALCULATIONS BELOW
    // ------------------------------------------------------------------------------------------------------------

    public float ForwardInLane(int _lane, float _startingValue, float distance)
    {
        return (_startingValue + distance) % laneManagerScript.LaneLengths[_lane];
    }

    public Vector3 GetPositionInLane(int _lane, float _newLaneValue)
    {
        return laneManagerScript.LaneScriptArray[_lane].path.GetPointAtDistance(_newLaneValue);
    }

    public Vector3 GetHeadingInLane(int _lane, float _newLaneValue)
    {
        Quaternion _rotationUpdate = laneManagerScript.LaneScriptArray[_lane].path.GetRotationAtDistance(_newLaneValue);

        Quaternion _rotationUpdatedAgain = _rotationUpdate * Quaternion.Euler(0, -90, 0);

        return _rotationUpdatedAgain * Vector3.right;
    }

    // _minThreshold = minimum range; optionalGuess = value that search will be centered on; optionalRangeLimiter = range divisor for faster calc
    public float ClosestPointOnPath(int _lane, Vector3 _carPosition, float _minThreshold, float _optionalGuess = 0f, int _optionalRangeLimiter = 1)
    {
        _runNum_d2l = 0;

        float _laneLength = laneManagerScript.LaneLengths[_lane];

        float _searchRange = _laneLength / _optionalRangeLimiter;

        float _min = (_optionalGuess + _laneLength - (_searchRange / 2)) % _laneLength;
        float _max = (_optionalGuess + _laneLength + (_searchRange / 2)) % _laneLength;

        float _bestGuess = RecursiveCalc(_min, _max, _searchRange, _carPosition, _laneLength, _lane, _minThreshold);

        return _bestGuess;
    }


    // min and max values to be passed into the search, range is between min and max
    public float RecursiveCalc(float _min, float _max, float _range, Vector3 _carPosition, float _laneLength, int _lane, float _minThreshold)
    {

        float _mid = (_min + (_range / 2)) % _laneLength;

        if (_range < _minThreshold)
        {
            return _mid;
        }

        float _midLower = (_mid + _laneLength - (_range / 4)) % _laneLength;
        float _midUpper = (_mid + (_range / 4)) % _laneLength;

        float _fMidLower = DistanceToLane(_midLower, _carPosition, _lane);
        float _fMidUpper = DistanceToLane(_midUpper, _carPosition, _lane);

        if (_fMidLower < _fMidUpper)
        {

            _runNum_d2l++;
            return RecursiveCalc(_min, _mid, _range / 2, _carPosition, _laneLength, _lane, _minThreshold);
        }

        else
        {

            _runNum_d2l++;
            return RecursiveCalc(_mid, _max, _range / 2, _carPosition, _laneLength, _lane, _minThreshold);
        }

    }

    public float Adjust(float _value, float _range, float _laneLength)
    {
        return (_value + _range) % _laneLength;
    }
    

    public float DistanceToLane(float _searchPosition, Vector3 _carPosition, int _lane)
    {
        Vector3 _lanePosition = laneManagerScript.LaneScriptArray[_lane].path.GetPointAtDistance(_searchPosition);

        return Vector3.Distance(_carPosition, _lanePosition);
    }




    // ------------------------------------------------------------------------------------------------------------
    // RECURSIVE PATH CALCULATOR BELOW
    // ------------------------------------------------------------------------------------------------------------




    public float CurveTrial(float _min, float _max, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio)
    {
        _runNum_rpc = 0;

        float _range = Mathf.Abs(_max - _min);

        if (_range < PathConstants.CURVE_FINDER_THRESHOLD)
        {
            return _min + (_range / 2);
        }

        float[,] _trialsArray = new float[PathConstants.SEARCH_DIVIDED, 2];


        int pos = 0;
        for (int i = 0; i < PathConstants.SEARCH_DIVIDED; i++)
        {
            

            float _value = _min + i * (_range / PathConstants.SEARCH_DIVIDED);
            float _fValue = CurveCalc(_value, A, D, _direction_A, _direction_B, _magRatio);
            _trialsArray[i, 0] = _value;
            _trialsArray[i, 1] = _fValue;

            // Get min in pos
            if (_trialsArray[i, 1] < _trialsArray[pos, 1]) { pos = i; }

            nodeList.Clear();
        }

        int _divisor = 8;

        float _newMin = _trialsArray[pos, 0] - (_range / _divisor);

        if (_newMin < 0) { _newMin = 0; }

        float _newMax = _trialsArray[pos, 0] + (_range / _divisor);

        _runNum_rpc++;

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


        for (float t = 0; t <= 1; t += PathConstants.CURVE_FINDER_INCREMENT)
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

        // The smallest distance between two nodes
        return 1 / nodeList.Min();

    }



    // ------------------------------------------------------------------------------------------------------------
    // MOVEMENT CALCULATOR BELOW
    // ------------------------------------------------------------------------------------------------------------



    public void MoveAlongPath(float dt, float L, float _t0, Vector3 _v1, Vector3 _v2, Vector3 _v3, Vector3 _heading, Vector3 _position)
    {

        float lag_threshold = 0.005f;
        //If we are experiencing a lag spike, iterate over a number of things
      
        //Add one, for min case
        int lag_runs = (int)Mathf.Round(dt / lag_threshold);

        if (lag_runs < 1)
            lag_runs = 1;

        float L_total = L;

        L /= lag_runs;

        for (int i = 0; i < lag_runs; i++)
        {
            //Update t0, based on the length of the movement and the bezier curve
            _t0 = T_Update(L, _t0, _v1, _v2, _v3);

            //Update new position pointing direction 
            _heading = Bezier_Heading(_t0, _v1, _v2, _v3);

            //Update carposition
            _position += _heading * L;
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

}
