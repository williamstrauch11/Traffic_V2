using UnityEngine;

[CreateAssetMenu(fileName = "CarScriptableObject", menuName = "ScriptableObjects/CarScriptableObject")]
public class CarScriptableObject : ScriptableObject
{
    
    // Units: m, kg

    public Sprite sprite;

    // https://www.decosoup.com/knowhow/1299-the-dimensions-of-an-one-car-and-a-two-car-garage
    public float Width; 
    public float Length;

    public float Velocity;

    public float mass; //kg

    public float FrictionCoefficient;

}
