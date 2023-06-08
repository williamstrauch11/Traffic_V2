using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LaneSetupScriptableObject", menuName = "ScriptableObjects/LaneSetupScriptableObject")]
public class LaneSetupScriptableObject : ScriptableObject
{
    [Header("CircularTrack")]
    public float NodeMultiplier; // Nodes per unit of radius
    public float TextureTilingMultiplier;
    public float InnerCircleRadius;

}
