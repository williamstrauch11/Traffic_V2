using UnityEngine;

[CreateAssetMenu(fileName = "CarFunctionsScriptableObject", menuName = "ScriptableObjects/CarFunctionsScriptableObject")]
public class CarFunctionsScriptableObject : ScriptableObject
{
    #region On Frame
    [Header("OnFrame:")]
    [Space(1)]
    [Header("MoveAlongPath")]

    [Range(0.0001f,0.005f)]
    [Tooltip("Break down each frame into incremental movements of this # of Seconds ")]
    public float LagThreshold;
    #endregion

    #region On Computation
    [Space(2)]
    [Header("OnComputation:")]

    [Range(0.001f, 1f)]
    [Tooltip("Distance (t) travelled on current path before new generation")]
    public float MaxPathTravel;

    [Space(1)]

    [Header("BezierParameterSet")]
    [Range(0.001f, 1f)]
    [Tooltip("Accuracy of where we are on lane. 1 =  Within 1 meter. ")]
    public float PointOnLaneThreshold;

    [Tooltip("How far away we make our new target point")]
    public float LaneJump;

    [Space(1)]
    [Header("NewBezierCalc")]

    [Range(2f, 16f)]
    [Tooltip("Distance between the two anchor points divided by this number to get search range for our new nodes. Essentially, limiting this search helps avoid weird results. Good value ~ 6")]
    public float CurveFinderLimiter;

    [Range(2f, 16f)]
    [Tooltip("Into how many parts do we divide each search?")]
    public int C_SearchDivisor;

    [Range(2f, 16f)]
    [Tooltip("When we find the min in one round, how far to the sides do we look on the next round? ( Previous range / #thisVariable# )")]
    public int C_SearchNarrower;

    [Range(0.001f, 0.1f)]
    [Tooltip("(m) when the range of our search gets this small, we return value")]
    public float C_CurveThreshold;

    [Range(0.001f, 0.1f)]
    [Tooltip("How many values for 't' do we use in our tried curve? (1 / #thisVariable#)")]
    public float CurveIncrement;

    [Space(2)]
    [Header("CurveAnalysis")]

    [Range(2f, 16f)]
    [Tooltip("Into how many parts do we divide each search?")]
    public int CA_SearchDivisor;

    [Range(2f, 16f)]
    [Tooltip("When we find the min in one round, how far to the sides do we look on the next round? ( Previous range / #thisVariable# )")]
    public int CA_SearchNarrower;

    [Range(0.001f, 0.050f)]
    [Tooltip("(m) when the range of our search gets this small, we return value")]
    public float CA_CurveThreshold;

    [Space(20)]

    [Range(0.001f, 0.1f)]
    [Tooltip("The portion of the curve (curve = 1) that we average, around max curvature, to get max curvature")]
    public float AC_Range;

    [Range(1f, 50f)]
    [Tooltip("Within our average range, # of values taken to average")]
    public int AC_Divisor;
    #endregion
}
