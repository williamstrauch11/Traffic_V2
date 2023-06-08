using System.Linq;
using UnityEngine;


public class PathFunctions
{
    #region Setup
    // References
    private LaneManagerScript laneManagerScript;
    public CarFunctionsScriptableObject carFunctionsScriptableObject { get; private set; }

    // Memory Allocation
    float[] C_nodeArray;
    float[,] C_trialsArray;
    float[,] CA_trialsArray;
    float[] AC_resultsArray;

    public PathFunctions()
    {
        // References fill
        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();
        carFunctionsScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/DefaultCarFunctions")) as CarFunctionsScriptableObject;

        // Memory Allocation
        C_nodeArray = new float[(int)(1 / carFunctionsScriptableObject.CurveIncrement) + 1];
        C_trialsArray = new float[carFunctionsScriptableObject.C_SearchDivisor, 2];
        CA_trialsArray = new float[carFunctionsScriptableObject.CA_SearchDivisor, 2];
        AC_resultsArray = new float[carFunctionsScriptableObject.AC_Divisor];
    }
    #endregion


    #region Lane Calculations

    public float ForwardInLane(int _lane, float _startingValue, float _distance)
    { 
        return (_startingValue + _distance) % laneManagerScript.LaneLengths[_lane];
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

        float _laneLength = laneManagerScript.LaneLengths[_lane];

        float _searchRange = _laneLength / _optionalRangeLimiter;

        float _min = (_optionalGuess + _laneLength - (_searchRange / 2)) % _laneLength;
        float _max = (_optionalGuess + _laneLength + (_searchRange / 2)) % _laneLength;

        float _bestGuess = CP_RecursiveCalc(_min, _max, _searchRange, _carPosition, _laneLength, _lane, _minThreshold);

        return _bestGuess;
    }


    /// <summary>
    /// Returns how close we are to the closest point of the given lane, within the accuracy given
    /// </summary>
    public float DistanceToLane(int _lane, Vector3 _position, float _accuracy)
    {

        float _pointInLAne = ClosestPointOnPath(_lane, _position, _accuracy);

        Vector3 _pointOnPath = GetPositionInLane(_lane, _pointInLAne);

        return Vector3.Distance(_position, _pointOnPath);

    }


    // min and max values to be passed into the search, range is between min and max
    // Good recursive format for symmetrical functions
    public float CP_RecursiveCalc(float _min, float _max, float _range, Vector3 _carPosition, float _laneLength, int _lane, float _minThreshold, int _runNum = 0)
    {

        float _mid = (_min + (_range / 2)) % _laneLength;

        if (_range < _minThreshold)
        {
            // Debug.Log(_runNum);

            return _mid;
        }

        float _midLower = (_mid + _laneLength - (_range / 4)) % _laneLength;
        float _midUpper = (_mid + (_range / 4)) % _laneLength;

        float _fMidLower = DistanceToLane(_midLower, _carPosition, _lane);
        float _fMidUpper = DistanceToLane(_midUpper, _carPosition, _lane);

        if (_fMidLower < _fMidUpper)
        {

            _runNum++;
            return CP_RecursiveCalc(_min, _mid, _range / 2, _carPosition, _laneLength, _lane, _minThreshold, _runNum);
        }

        else
        {

            _runNum++;
            return CP_RecursiveCalc(_mid, _max, _range / 2, _carPosition, _laneLength, _lane, _minThreshold, _runNum);
        }

    }

    public float DistanceToLane(float _searchPosition, Vector3 _carPosition, int _lane)
    {
        Vector3 _lanePosition = laneManagerScript.LaneScriptArray[_lane].path.GetPointAtDistance(_searchPosition);

        return Vector3.Distance(_carPosition, _lanePosition);
    }
    #endregion


    #region Path Generation

    public float C_CurveGenerator(float _xMin, float _xMax, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio, int _runNum = 0)
    {
        int _searchDivisor = carFunctionsScriptableObject.C_SearchDivisor;
        int _searchNarrower = carFunctionsScriptableObject.C_SearchNarrower;
        float _searchThreshold = carFunctionsScriptableObject.C_CurveThreshold;


        float _xRange = Mathf.Abs(_xMax - _xMin);

        if (_xRange < _searchThreshold)
        {
            // Debug.Log(_runNum);

            return _xMin + (_xRange / 2);
        }


        int pos = 0;
        for (int i = 0; i < _searchDivisor; i++)
        {
            

            float _xValue = _xMin + i * (_xRange / _searchDivisor);
            float _yValue = C_CurveSetup(_xValue, A, D, _direction_A, _direction_B, _magRatio);
            C_trialsArray[i, 0] = _xValue;
            C_trialsArray[i, 1] = _yValue;

            // Get min in pos
            if (C_trialsArray[i, 1] < C_trialsArray[pos, 1]) { pos = i; }

        }

        float _xMinNew = C_trialsArray[pos, 0] - (_xRange / _searchNarrower);

        if (_xMinNew < 0) { _xMinNew = 0; }

        float _xMaxNew = C_trialsArray[pos, 0] + (_xRange / _searchNarrower);

        _runNum++;

        return C_CurveGenerator(_xMinNew, _xMaxNew, A, D, _direction_A, _direction_B, _magRatio, _runNum);

    }

    // Set up curves to be tested
    public float C_CurveSetup(float _factor, Vector3 A, Vector3 D, Vector3 _direction_A, Vector3 _direction_B, float _magRatio)
    {

        Vector3 Trial_A = A + (_direction_A * _magRatio * _factor);
        Vector3 Trial_B = D + (_direction_B * _factor);

        return C_TestCurve(A, Trial_A, Trial_B, D);
    }

    // Test each curve
    public float C_TestCurve(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {

        Vector3 _oldPoint;
        Vector3 _point = new Vector3();

        int i = 0;
        for (float t = 0; t <= 1; t += carFunctionsScriptableObject.CurveIncrement)
        {
            _oldPoint = _point;

            _point = Mathf.Pow(1 - t, 3) * A +
                3 * Mathf.Pow(1 - t, 2) * t * B +
                3 * (1 - t) * Mathf.Pow(t, 2) * C +
                Mathf.Pow(t, 3) * D;
            
            float _distance = Vector3.Distance(_oldPoint, _point);

            C_nodeArray[i] = _distance;

            i++;
        }

        // The smallest distance between two nodes
        return 1 / C_nodeArray.Min();

    }
    #endregion


    #region Movement Calculator

    public float MAP_TimerUpdate(float L, float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // float t_update = t + L / Vector3.Magnitude((Mathf.Pow(t, 2) * v1) + (t * v2 + v3));
        return t + L / Vector3.Magnitude(Mathf.Pow(t, 2) * v1 + t * v2 + v3);

    }
    #endregion


    #region CurveAnalysis

    // returns float array. [0] = Result (x), [1] = runNum
    // _searchDivisor: How many searches between _xMin and _xMax do we conduct each run.
    // _searchNarrower: How much smaller do we make the next run? ( 1 / value )
    // _searchThreshold: Minimum _xRange when we abort search
    public float CA_RecursiveDivided(float _xMin, float _xMax, Vector3 _v1, Vector3 _v2, Vector3 _v3, int _runNum = 0, float _xLowerBound = 0, float _xUpperBound = 0)
    {

        int _searchDivisor = carFunctionsScriptableObject.CA_SearchDivisor;
        int _searchNarrower = carFunctionsScriptableObject.CA_SearchNarrower;
        float _searchThreshold = carFunctionsScriptableObject.CA_CurveThreshold;

        if (_runNum == 0)
        {
            _xLowerBound = _xMin;
            _xUpperBound = _xMax;
        }

        float _xRange = Mathf.Abs(_xMax - _xMin);

        if (_xRange < _searchThreshold)
        {
            //Debug.Log(_xMin);
            return _xMin + (_xRange / 2);
                
        }

        int pos = 0;
        for (int i = 0; i < _searchDivisor; i++)
        {

            float _xValue = _xMin + i * (_xRange / _searchDivisor) + (_xRange / (_searchDivisor * 2));
            float _yValue = CurvatureAt(_xValue, _v1, _v2, _v3);

            CA_trialsArray[i, 0] = _xValue;
            CA_trialsArray[i, 1] = _yValue;

            // Get min in pos
            if (CA_trialsArray[i, 1] > CA_trialsArray[pos, 1]) { pos = i; }
        }

        // Narrow down our _xRange
        float _xNewMin = CA_trialsArray[pos, 0] - (_xRange / _searchNarrower);

        if (_xNewMin < _xLowerBound) { _xNewMin = _xLowerBound; }

        float _xNewMax = CA_trialsArray[pos, 0] + (_xRange / _searchNarrower);

        if (_xNewMax > _xUpperBound) { _xNewMax = _xUpperBound; }

        _runNum++;

        return CA_RecursiveDivided(_xNewMin, _xNewMax, _v1, _v2, _v3, _runNum, _xLowerBound, _xUpperBound);

    }

    public void CurveAnalysis(Vector3 _v1, Vector3 _v2, Vector3 _v3, float _velocity, float _frictionCoefficient)
    {

        float _lowerBound = 0f;
        float _upperBound = 1f;

        float _result = CA_RecursiveDivided(_lowerBound, _upperBound, _v1, _v2, _v3);

        float _maxCurve = AverageCurvatureAt(_result, 0.05f, _v1, _v2, _v3);


        // Given by: https://shorturl.at/cCM37
        // Note: mass has fallen out of the calculation 
        // maxVelocity = SQRT ( FrictionCoefficient * g * radius )

        // radius is our result, so velocity ^ 2 / _frictionCoefficient ~ result

        float _leftSide = Mathf.Pow(_velocity, 2) / _frictionCoefficient;
        float _rightSide = RunSettings.MAX_TURN_RADIUS_CONSTANT / _maxCurve;

        // Debug.Log("leftside  " + _leftSide + "  rightside  " + _rightSide);

        if (_leftSide > _rightSide)
        {
            Debug.Log("SKID");
        }
        

    }

    public float AverageCurvatureAt(float t, float _range, Vector3 v1, Vector3 v2, Vector3 v3)
    {

        float _xMin = t - (_range / 2);
        float _xMax = t + (_range / 2);

        if (_xMin < 0f)

        {
            _xMin = 0f;

        }
        else if (_xMax > 1)
        {

            _xMin = 1 - _range;
        }

        int _divisor = 10;

        for (int i = 0; i < _divisor; i++)
        {

            float _xValue = _xMin + i * (_range / _divisor) + (_range / (_divisor * 2));

            // Debug.Log("x  " + _xValue);

            AC_resultsArray[i] = CurvatureAt(_xValue, v1, v2, v3);

            // Debug.Log("y  " + _resultsArray[i]);

        }

        return AC_resultsArray.Average();
    }

    public float CurvatureAt(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 d1 = BezFirstDerivative(t, v1, v2, v3);
        Vector3 d2 = BezSecondDerivative(t, v1, v2, v3);

        return BezCurvature(d1, d2);
    }
    #endregion


    #region General Calculations
    public Vector3 BezFirstDerivative(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return (Mathf.Pow(t, 2) * v1) + (t * v2) + v3;
    }

    public Vector3 BezSecondDerivative(float t, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return (2 * t * v1) + v2;
    }

    public float BezCurvature(Vector3 d1, Vector3 d2)
    {
        return Vector3.Magnitude(Vector3.Cross(d1, d2)) / (Mathf.Pow(Vector3.Magnitude(d1), 3));
    }
    #endregion
}
