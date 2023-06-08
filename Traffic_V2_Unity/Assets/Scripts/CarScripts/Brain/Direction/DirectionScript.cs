using UnityEngine;

public class DirectionScript
{

    #region Setup
    // Positional Variables
    public Vector3 Position { get; private set; }
    public Vector3 Heading { get; private set; }

    // Path Nodes
    public Vector3 Path_Anchor_1 { get; private set; }
    public Vector3 Path_Anchor_2 { get; private set; }
    public Vector3 Path_Direction_1 { get; private set; }
    public Vector3 Path_Direction_2 { get; private set; }

    // Path Vectors
    public Vector3 PV1 { get; private set; }
    public Vector3 PV2 { get; private set; }
    public Vector3 PV3 { get; private set; }

    // Path Variables
    public float PathTimer { get; private set; }
    public int _localLaneChanger { get; private set; }

    // References
    public CarFunctionsScriptableObject carFunctionsScriptableObject { get; private set; }
    public CarScriptableObject carScriptableObject { get; private set; }
    public PathFunctions pathFunctions;


    public DirectionScript()
    {
        // References fill
        carFunctionsScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/DefaultCarFunctions")) as CarFunctionsScriptableObject;
        carScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/DefaultCar")) as CarScriptableObject;
        pathFunctions = new PathFunctions();

        PathTimer = 0f;
        _localLaneChanger = 0;

    }
#endregion


    #region On Computation
    /// <summary>
    /// Checks to see if we need to form a new path
    /// </summary>
    public void OnComputation(int _lane, Vector3 _position, Vector3 _heading, float _velocity, bool _startup)
    {
        if (PathTimer > carFunctionsScriptableObject.MaxPathTravel)
        {
            if (_localLaneChanger == _lane)
            {
                _localLaneChanger = 1;
            }
            else
            {
                _localLaneChanger = _lane;
            }

            BezierParameterSet(_localLaneChanger, _velocity, _position, _heading);

        }

        else if (_startup)
        {
            BezierParameterSet(_lane, _velocity, _position, _heading);
        }

        
        // This may mean check to see how far away we are from our lane, and how we are oriented relative to it
        // Also, if we are getting close to the end of our computed path

        // if everything is ok, do nothing
        // If not, compute a new path
    }
    #endregion


    #region On Frame
    public void OnFrame(float L, float dt)
    {
        // Move along our current path the distance L
        MoveAlongPath(dt, L, PV1, PV2, PV3);
    }
    #endregion


    #region General Methods
    /// <summary>
    /// Given a lane and a position on that lane, simply place the car facing foward on that lane
    /// </summary>
    public void PutOnLane(int _lane, float _lanePosition)
    {
        Position = pathFunctions.GetPositionInLane(_lane, _lanePosition);
        Heading = pathFunctions.GetHeadingInLane(_lane, _lanePosition);
    }
    #endregion


    #region New Path Calc
    /// <summary>
    /// Gets all of the neccesary parameters for a NewBezierCalc, then calls NewBezierCalc
    /// </summary>
    public void BezierParameterSet(int _lane, float _velocity, Vector3 _position, Vector3 _heading)
    {

        float _currentLaneValue = pathFunctions.ClosestPointOnPath(_lane, _position, carFunctionsScriptableObject.PointOnLaneThreshold);

        float _newLaneValue = pathFunctions.ForwardInLane(_lane, _currentLaneValue, carFunctionsScriptableObject.LaneJump);

        Vector3 _targetPosition = pathFunctions.GetPositionInLane(_lane, _newLaneValue);

        Vector3 _targetHeading = Quaternion.Euler(0, 0, 180) * pathFunctions.GetHeadingInLane(_lane, _newLaneValue); // Get heading, rotate it backwards

        NewBezierCalc(_position, _targetPosition, _heading, _targetHeading, _velocity, _velocity);
    }

    
    /// <summary>
    /// Recursively generates the ideal bezier curve based on the vector and velocity inputs;
    /// Script-wide path descriptive variables will update at the end
    /// </summary>
    private void NewBezierCalc(Vector3 _currentPosition, Vector3 _finalPosition, Vector3 _currentHeading, Vector3 _finalHeading, float _currentVelocity, float _finalVelocity)
    {

        Vector3 _anchor1 = _currentPosition;
        Vector3 _anchor2 = _finalPosition;

        Vector3 _direction1 = _currentHeading;
        Vector3 _direction2 = _finalHeading;

        float _magRatio = _currentVelocity / _finalVelocity;

        float TrialDistance = Vector3.Distance(_anchor1, _anchor2) / carFunctionsScriptableObject.CurveFinderLimiter; // 6 seems to be fine, all resulting values fall within the range

        // Recurse!
        float FinalDistance = pathFunctions.C_CurveGenerator(0f, TrialDistance * 2, _anchor1, _anchor2, _direction1, _direction2, _magRatio);

        Path_Anchor_1 = _anchor1;
        Path_Anchor_2 = _anchor2;

        Path_Direction_1 = _anchor1 + (_direction1 * _magRatio * FinalDistance);
        Path_Direction_2 = _anchor2 + (_direction2 * FinalDistance);

        // Compute three vectors from the derivative of the bezier curve
        CurveVectorCalc(Path_Anchor_1, Path_Direction_1, Path_Direction_2, Path_Anchor_2);

        pathFunctions.CurveAnalysis(PV1, PV2, PV3, _currentVelocity, carScriptableObject.FrictionCoefficient);

        PathTimer = 0;
    }


    /// <summary>
    /// Updates the three path vectors (PV1,2,3)
    /// </summary>
    public void CurveVectorCalc(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        PV1 = (-3 * A) + (9 * B) - (9 * C) + (3 * D);
        PV2 = (6 * A) - (12 * B) + (6 * C);
        PV3 = (-3 * A) + (3 * B);
    }
    #endregion


    #region Path Movement
    /// <summary>
    /// Updates Position and Heading after moving distance L along the curve
    /// </summary>
    private void MoveAlongPath(float dt, float L, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // If we are experiencing a lag spike, iterate over a number of things
        
        // Add one, for min case
        int lag_runs = (int)Mathf.Round(dt / carFunctionsScriptableObject.LagThreshold);

        if (lag_runs < 1)
            lag_runs = 1;

        L /= lag_runs;

        for (int i = 0; i < lag_runs; i++)
        {
            // Reference: https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

            // Update PathTimer, based on the length of the movement and the bezier curve
            PathTimer = pathFunctions.MAP_TimerUpdate(L, PathTimer, v1, v2, v3);

            // Update new position pointing direction 
            Heading = Vector3.Normalize(pathFunctions.BezFirstDerivative(PathTimer, v1, v2, v3));

            // Update carposition
            Position += Heading * L;

        }
        
    }
    #endregion

}