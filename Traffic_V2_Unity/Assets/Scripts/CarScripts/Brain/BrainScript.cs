using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainScript
{
    // Fields
    // public float Delay { get; }

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


    public void RunBrain()
    {

        directionScript.OnComputation();
        accelerationScript.OnComputation();

    }

}
