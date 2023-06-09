using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

//I found this 'Editor' script setup from the video on creating a field of view: https://www.youtube.com/watch?v=j1-OyLo77ss&ab_channel=Comp-3Interactive

[CustomEditor(typeof(CarScript))]

public class VisionEditor : Editor
{
    // References
    VisionEditorHelper visionEditorHelper;

    private void OnEnable()
    {
        visionEditorHelper = new VisionEditorHelper();
    }

    private void OnSceneGUI()
    {

        //'target' here seems local- not to be confused with the local target used in FieldofView.cs
        CarScript car = (CarScript)target;

    }

    
}
