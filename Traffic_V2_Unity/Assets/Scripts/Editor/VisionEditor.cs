using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//I found this 'Editor' script setup from the video on creating a field of view: https://www.youtube.com/watch?v=j1-OyLo77ss&ab_channel=Comp-3Interactive

[CustomEditor(typeof(VisionScript))]

public class FieldOfViewEditor : Editor
{
    /*
     * 
    private void OnSceneGUI()
    {
        //'target' here seems local- not to be confused with the local target used in FieldofView.cs
        VisionScript fov = (VisionScript)target;


        //Draw the wide perception cones
        DrawPie(Color.white, fov.driverPosition, fov.radiusScanMin, fov.angleScanMin, fov.driverHeading);
        DrawPie(Color.white, fov.driverPosition, fov.radiusScanMid, fov.angleScanMid, fov.driverHeading);
        DrawPie(Color.white, fov.driverPosition, fov.radiusScanMax, fov.angleScanMax, fov.driverHeading);


        //Draw the narrow attention cones
        DrawPie(Color.blue, fov.driverPosition, fov.radiusVisionMax, fov.angleVisionMax, fov.driverHeading);
        DrawPie(Color.blue, fov.driverPosition, fov.radiusVisionMid, fov.angleVisionMid, fov.driverHeading);
        DrawPie(Color.blue, fov.driverPosition, fov.radiusVisionMin, fov.angleVisionMin, fov.driverHeading);


        //Draw the rearview mirror
        DrawPie(Color.yellow, fov.rearviewMirrorPosition, fov.radiusVisionMax, fov.angleRearView, fov.rearviewMirrorHeading);


        //Draw the two side mirrors
        DrawPie(Color.white, fov.leftMirrorPosition, fov.radiusVisionMax, fov.angleSide, fov.leftMirrorHeading);
        DrawPie(Color.white, fov.rightMirrorPosition, fov.radiusVisionMax, fov.angleSide, fov.rightMirrorHeading);

    }

    private void DrawPie(Color color, Vector3 origin, float radius, float angle, Vector3 heading)
    {

        float internalOffset = angle / 2f; //Half of the cone, as an angle 

        float zEuler = 270f + Vector3.Angle(Vector3.up, heading); //Find angle between y axis and heading (the if statement is to get the angles form 0 to 360)

        if (heading.x > 0)
        {
            zEuler = 90f + Vector3.Angle(Vector3.down, heading);
        }

        Vector3 viewAngle01 = Quaternion.AngleAxis(internalOffset, Vector3.forward) * heading;
        Vector3 viewAngle02 = Quaternion.AngleAxis(-internalOffset, Vector3.forward) * heading;

        Handles.color = color;

        Handles.DrawWireArc(origin, Vector3.forward, viewAngle02, angle, radius);

        Handles.DrawLine(origin, origin + viewAngle01 * radius);
        Handles.DrawLine(origin, origin + viewAngle02 * radius);
    }
    */
}
