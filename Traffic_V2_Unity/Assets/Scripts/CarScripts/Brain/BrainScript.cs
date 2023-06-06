using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainScript
{
    

    // References
    public VisionScript visionScript;
    public DirectionScript directionScript;
    public AccelerationScript accelerationScript;




    public BrainScript()
    {
        // References fill
        visionScript = new VisionScript();
        directionScript = new DirectionScript();
        accelerationScript = new AccelerationScript();

    }


    public void RunBrain(int _lane, Vector3 _position, Vector3 _heading, float _velocity)
    {

        directionScript.OnComputation(_lane, _position, _heading, _velocity);
        accelerationScript.OnComputation();

    }

}
