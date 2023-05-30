using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneFieldManager
{
    public LaneFieldManagerVariables laneFieldManagerVariables = new LaneFieldManagerVariables();

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
        _radius = (LaneNum * RunSettings.LANE_WIDTH * 2) + RunSettings.INNER_LANE_RADIUS;
        _roadWidth = RunSettings.LANE_WIDTH;
        _nodeNum = (int)Mathf.Round(_radius * laneFieldManagerVariables._nodeMultiplier);
        _textureTiling = laneFieldManagerVariables._textureTilingMultiplier * _radius;
        
    }

}

public class LaneFieldManagerVariables
{
    public float _nodeMultiplier { get; } = 0.5f; // Nodes per unit of radius
    public float _textureTilingMultiplier { get; } = 0.2f;
}
