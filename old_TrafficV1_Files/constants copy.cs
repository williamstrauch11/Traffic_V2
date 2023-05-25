using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constants : MonoBehaviour
{
    static public int CarNum = 30;

    static public int AttributeNum = 3;

    static public float CarLength = 7.5f;

    static public float CarWidth = 3.4f;

    //Lane Change
    static public int nodeNum = 4;

    //Cars
    static public float SpawnSpeed = 0;

    //Motion
    static public float accelChangeThreshold = 50f;

    //Car Position Vectors
    static public Vector3 topLeft = new Vector3(-constants.CarWidth / 2, constants.CarLength / 2, 0f);
    static public Vector3 topMiddle = new Vector3(0f, constants.CarLength / 2, 0f);
    static public Vector3 topRight = new Vector3(constants.CarWidth / 2, constants.CarLength / 2, 0f);
    static public Vector3 rightMiddle = new Vector3(constants.CarWidth / 2, 0f, 0f);
    static public Vector3 backRight = new Vector3(constants.CarWidth / 2, -constants.CarLength / 2, 0f);
    static public Vector3 backMiddle = new Vector3(0f, -constants.CarLength / 2, 0f);
    static public Vector3 backLeft = new Vector3(-constants.CarWidth / 2, -constants.CarLength / 2, 0f);
    static public Vector3 leftMiddle = new Vector3(-constants.CarWidth / 2, 0f, 0f);

    //Functions
    static public Vector3 OffsetCalc(Vector3 heading, Vector3 position, Vector3 offsetVect) //offsetVect is for a car pointed along the y axis 
    {
        heading = Vector3.ProjectOnPlane(heading, Vector3.forward); // project heading vector to xy plane
        offsetVect = Vector3.ProjectOnPlane(offsetVect, Vector3.forward); //project offset vector to xy plane

        float angleBetween = Vector3.Angle(Vector3.up, heading); //Find angle between y axis and heading (the if statement is to get the angles form 0 to 360)

        if (heading.x > 0)
        {
            angleBetween = 180f + Vector3.Angle(Vector3.down, heading);
        }

        offsetVect = Quaternion.AngleAxis(angleBetween, Vector3.forward) * offsetVect; // Rotate our offset vector to stay aligned with the heading vector

        return position = position + offsetVect; //return the new position
    }

    static public float SumArray(float[] toBeSummed)
    {
        float sum = 0f;

        foreach (int item in toBeSummed)
        {
            sum += item;
        }

        return sum;
    }

}