using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarScriptableObject", menuName = "ScriptableObjects/CarScriptableObject")]
public class CarScriptableObject : ScriptableObject
{
    // https://www.decosoup.com/knowhow/1299-the-dimensions-of-an-one-car-and-a-two-car-garage
    // Units: m, kg
    public float Width = 1.9f; 
    public float Length = 4.7f;

    public float Velocity = 10f; 

    public float mass = 1300; //kg

}
