using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovementScript
{
    public Vector3 Position;
    public Vector3 Heading;
    public float Velocity;


    CarMovementScript()
    {
        return;
    }

    public float Move(float _acceleration, float dt)
    {

        Velocity += _acceleration * dt;

        return (Velocity * dt) + (_acceleration * dt * dt / 2);

    }


}
