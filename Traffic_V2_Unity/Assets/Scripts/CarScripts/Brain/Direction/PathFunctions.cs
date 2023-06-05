using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PathFunctions
{
    private LaneManagerScript laneManagerScript;
    public int RunNum;

    public PathFunctions()
    {

        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();

    }

    // _minThreshold = minimum range; optionalGuess = value that search will be centered on; optionalRangeLimiter = range divisor for faster calc
    public float ClosestPointOnPath(int _lane, Vector3 _carPosition, float _minThreshold, float _optionalGuess = 0f, int _optionalRangeLimiter = 1)
    {
        RunNum = 0;

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

            RunNum++;
            return RecursiveCalc(_min, _mid, _range / 2, _carPosition, _laneLength, _lane, _minThreshold);
        }

        else
        {

            RunNum++;
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

    
}
