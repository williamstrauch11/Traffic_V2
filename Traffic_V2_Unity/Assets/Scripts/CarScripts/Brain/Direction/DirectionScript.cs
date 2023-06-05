using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionScript
{
    public Vector3 Position { get; private set; }
    public Vector3 Heading { get; private set; }

    public PathFunctions pathFunctions;

    public DirectionScript()
    {
        pathFunctions = new PathFunctions();
    }

    public void OnComputation()
    {

    }

    public void OnFrame(float L, Vector3 _position)
    {

    }

    public float GetCurrentLane(Vector3 _position)
    {
        return 0f;
    }

    public void BezierCalc(float _targetLane, float _targetArriveBy)
    {

    }
   

    public void NewBezierCalc(Vector3 _currentPosition, Vector3 _finalPosition, Vector3 _currentHeading, Vector3 _finalHeading, float _currentVelocity, float _finalVelocity)
    {

    }
}
