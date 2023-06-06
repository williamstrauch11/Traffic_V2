using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DirectionScript
{
    // State Variables
    public Vector3 Position { get; private set; }
    public Vector3 Heading { get; private set; }

    // Path Nodes
    public Vector3 Path_Anchor_1 { get; private set; }
    public Vector3 Path_Anchor_2 { get; private set; }
    public Vector3 Path_Direction_1 { get; private set; }
    public Vector3 Path_Direction_2 { get; private set; }

    // Path Derivative Vectors
    public Vector3 PDV1 { get; private set; }
    public Vector3 PDV2 { get; private set; }
    public Vector3 PDV3 { get; private set; }

    // Path Variables
    public float PathTimer { get; private set; }
    public int _localLaneChanger { get; private set; }

    public PathFunctions pathFunctions;

    public DirectionScript()
    {
        pathFunctions = new PathFunctions();
        PathTimer = 0f;
        _localLaneChanger = 1;
    }

    public void OnComputation(int _lane, Vector3 _position, Vector3 _heading, float _velocity, bool _startup)
    {
        if (PathTimer > PathConstants.TIMER_MAX)
        {
            BezierCalc(_lane, _velocity, _position, _heading);
        }

        else if (_startup)
        {
            Debug.Log("Startup");
            BezierCalc(_lane, _velocity, _position, _heading);
        }

        
        // This may mean check to see how far away we are from our lane, and how we are oriented relative to it
        // Also, if we are getting close to the end of our computed path

        // if everything is ok, do nothing
        // If not, compute a new path
    }

    public void OnFrame(float L, float dt)
    {
        // Move along our current path the distance L
        MoveAlongPath(dt, L, PDV1, PDV2, PDV3);


    }

    public void SpawnSet(int _lane, float _lanePosition)
    {
        Position = pathFunctions.GetPositionInLane(_lane, _lanePosition);
        Heading = pathFunctions.GetHeadingInLane(_lane, _lanePosition);
    }

    public float LaneProximityCheck(int _lane, Vector3 _position)
    {
        float _accuracy = 0.001f;
        float _pointInLAne = pathFunctions.ClosestPointOnPath(_lane, _position, _accuracy);

        Vector3 _pointOnPath = pathFunctions.GetPositionInLane(_lane, _pointInLAne);

        return Vector3.Distance(_position, _pointOnPath);

    }

    public void BezierCalc(int _lane, float _velocity, Vector3 _position, Vector3 _heading)
    {

        float _currentLaneValue = pathFunctions.ClosestPointOnPath(_lane, _position, 0.01f);

        float _newLaneValue = pathFunctions.ForwardInLane(_lane, _currentLaneValue, 100f);

        Vector3 _targetPosition = pathFunctions.GetPositionInLane(_lane, _newLaneValue);

        Vector3 _targetHeading = Quaternion.Euler(0, 0, 180) * pathFunctions.GetHeadingInLane(_lane, _newLaneValue); // Get heading, rotate it backwards

        NewBezierCalc(_position, _targetPosition, _heading, _targetHeading, _velocity, _velocity);
    }


    private void NewBezierCalc(Vector3 _currentPosition, Vector3 _finalPosition, Vector3 _currentHeading, Vector3 _finalHeading, float _currentVelocity, float _finalVelocity)
    {

        Vector3 _anchor1 = _currentPosition;
        Vector3 _anchor2 = _finalPosition;

        Vector3 _direction1 = _currentHeading;
        Vector3 _direction2 = _finalHeading; // Finalheading needs to be flipped

        float _mag1 = _currentVelocity;
        float _mag2 = _finalVelocity;

        float _magRatio = _mag1 / _mag2;

        float TrialDistance = Vector3.Distance(_anchor1, _anchor2) / PathConstants.CURVE_FINDER_LIMITER; // 6 seems to be fine, all resulting values fall within the range


        // Recurse!
        float FinalDistance = pathFunctions.CurveTrial(0f, TrialDistance * 2, _anchor1, _anchor2, _direction1, _direction2, _magRatio);

        Path_Anchor_1 = _anchor1;
        Path_Anchor_2 = _anchor2;

        Path_Direction_1 = _anchor1 + (_direction1 * _magRatio * FinalDistance);
        Path_Direction_2 = _anchor2 + (_direction2 * FinalDistance);

        // Compute three vectors from the derivative of the bezier curve
        PDV1 = (-3 * Path_Anchor_1) + (9 * Path_Direction_1) - (9 * Path_Direction_2) + (3 * Path_Anchor_2);
        PDV2 = (6 * Path_Anchor_1) - (12 * Path_Direction_1) + (6 * Path_Direction_2);
        PDV3 = (-3 * Path_Anchor_1) + (3 * Path_Direction_1);

        Debug.Log("PathUpdate");

        PathTimer = 0;
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
            PathTimer = pathFunctions.T_Update(L, PathTimer, _v1, _v2, _v3);

            //Update new position pointing direction 
            Heading = pathFunctions.Bezier_Heading(PathTimer, _v1, _v2, _v3);

            //Update carposition
            Position += Heading * L;

        }
        
    }
        
}