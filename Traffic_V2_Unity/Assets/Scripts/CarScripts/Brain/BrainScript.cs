using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainScript
{
    #region References and Constructor
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
    #endregion

    public void RunBrain(int _lane, Vector3 _position, Vector3 _heading, float _velocity, bool _startup = false)
    {

        directionScript.OnComputation(_lane, _position, _heading, _velocity, _startup);
        accelerationScript.OnComputation();

    }

}
