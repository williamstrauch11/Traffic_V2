using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneFieldManager
{
    public int LaneNum { get; }
    public float Radius { get; }
    public float RoadWidth { get; }
    public int NodeNum { get; }
    public float TextureTiling { get; }

    private float _radius;
    private float _roadWidth;
    private int _nodeNum;
    private float _textureTiling;


    public LaneFieldManager(int _laneNum)
    {
        LaneNum = _laneNum;

        SetParameters();

        Radius = _radius;
        RoadWidth = _roadWidth;
        NodeNum = _nodeNum;
        TextureTiling = _textureTiling;
    }

    public void SetParameters()
    {
        _radius = 10;
        _roadWidth = 0.5f;
        _nodeNum = 101;
        _textureTiling = 30;
    }

}
