using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LaneSetupScriptableObject", menuName = "ScriptableObjects/LaneSetupScriptableObject")]
public class LaneSetupScriptableObject : ScriptableObject
{
    [Header("CircularTrack")]
    public float NodeMultiplier = 0.5f; // Nodes per unit of radius
    public float TextureTilingMultiplier = 0.2f;
    public float InnerCircleRadius = 100f;

}
