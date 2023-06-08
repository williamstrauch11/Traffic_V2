using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LaneFieldManager
{

    public LaneSetupScriptableObject laneSetupScriptableObject;

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

        laneSetupScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/LaneSetup1")) as LaneSetupScriptableObject;

        SetParameters();

        Radius = _radius;
        RoadWidth = _roadWidth;
        NodeNum = _nodeNum;
        TextureTiling = _textureTiling;
    }

    public void SetParameters()
    {
        _radius = (LaneNum * RunSettings.LANE_WIDTH * 2) + laneSetupScriptableObject.InnerCircleRadius;
        _roadWidth = RunSettings.LANE_WIDTH;
        _nodeNum = (int)Mathf.Round(_radius * laneSetupScriptableObject.NodeMultiplier);
        _textureTiling = laneSetupScriptableObject.TextureTilingMultiplier * _radius;
        
    }

}


