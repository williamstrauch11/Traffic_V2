using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//I found this 'Editor' script setup from the video on creating a field of view: https://www.youtube.com/watch?v=j1-OyLo77ss&ab_channel=Comp-3Interactive

[CustomEditor(typeof(CarScript))]

public class CarScriptEditor : Editor
{
    // References
    PathEditorHelper pathEditorHelper;

    private void OnEnable()
    {
        pathEditorHelper = new PathEditorHelper();
    }

    private void OnSceneGUI()
    {

        //'target' here seems local- not to be confused with the local target used in FieldofView.cs
        CarScript car = (CarScript)target;

        // Handles.DrawWireArc(car.brainScript.directionScript.Position, Vector3.forward, Vector3.right, 360, 50);

        Vector3 A = car.brainScript.directionScript.Path_Anchor_1;
        Vector3 B = car.brainScript.directionScript.Path_Direction_1;
        Vector3 C = car.brainScript.directionScript.Path_Direction_2;
        Vector3 D = car.brainScript.directionScript.Path_Anchor_2;

        CurveDraw(A, B, C, D);
        NodeDraw(A, B, C, D);
    }

    public void CurveDraw(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {

        Handles.color = Color.white;

        for (float t = 0; t <= 1; t += PathConstants.CURVE_FINDER_INCREMENT)
        {


            Vector3 _point = Mathf.Pow(1 - t, 3) * A +
                3 * Mathf.Pow(1 - t, 2) * t * B +
                3 * (1 - t) * Mathf.Pow(t, 2) * C +
                Mathf.Pow(t, 3) * D;

            Handles.DrawWireCube(_point, new Vector3(0.2f, 0.2f, 0.2f));

        }


    }

    public void NodeDraw(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        Handles.color = Color.red;

        Handles.DrawWireCube(A, new Vector3(1, 1, 1));
        Handles.DrawWireCube(B, new Vector3(1, 1, 1));
        Handles.DrawWireCube(C, new Vector3(1, 1, 1));
        Handles.DrawWireCube(D, new Vector3(1, 1, 1));
    }

}
