using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript
{
    public float Velocity { get; private set; }

    public MovementScript(float _velocity)
    {
        Velocity = _velocity;
    }

    public float MovementCalc(float _acceleration, float dt)
    {

        
        if (Velocity < 25)
        {
            Velocity += _acceleration * dt;
        }
       

        return (Velocity * dt) + (_acceleration * dt * dt / 2);
    }
}
