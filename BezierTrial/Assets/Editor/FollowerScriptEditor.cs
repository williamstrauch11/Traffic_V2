using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;

//I found this 'Editor' script setup from the video on creating a field of view: https://www.youtube.com/watch?v=j1-OyLo77ss&ab_channel=Comp-3Interactive

[CustomEditor(typeof(FollowerScript))]

public class FollowerScriptEditor : Editor
{

    private void OnSceneGUI()
    {
        /*
            
        //'target' here seems local- not to be confused with the local target used in FieldofView.cs
        FollowerScript car = (FollowerScript)target;

        Gizmos.color = Color.white; 

        CurveDraw(car.A, car.B, car.C, car.D);
        */
    }

    public void CurveDraw(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {

        for (float t = 0; t <= 1; t += 0.005f)
        {
            

            Vector3 _point = Mathf.Pow(1 - t, 3) * A +
                3 * Mathf.Pow(1 - t, 2) * t * B +
                3 * (1 - t) * Mathf.Pow(t, 2) * C +
                Mathf.Pow(t, 3) * D;

            Gizmos.DrawWireSphere(_point, 0.1f);


        }

    }
}