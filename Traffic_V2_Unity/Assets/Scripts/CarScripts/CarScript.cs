using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class CarScript : MonoBehaviour
{
    // References
    private LaneManagerScript laneManagerScript; 
    private CarScriptableObject carScriptableObject;
    private BoxCollider boxCollider;
    private BrainScript brainScript;

    // Properties
    [field: SerializeField] public int CarID { get; private set; }
    [field: SerializeField] public int Lane { get; private set; }

    // Positional
    public Vector3 Position { get; private set; }
    public Vector3 Heading { get; private set; }
    [field: SerializeField] public float LanePosition { get; private set; }
    public float LanePositionCalc;
    public float Velocity { get; private set; }



    // Start is called before the first frame update
    void Awake()
    {
        // References fill
        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();
        carScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/CarScriptableObject")) as CarScriptableObject;
        boxCollider = this.gameObject.GetComponent<BoxCollider>();

    }

    public void Run(float dt)
    {

        /*
        // Get acceleration
        float _acceleration = brainScript.accelerationScript.OnFrame(dt);

        // Calculate L (distance travelled in this frame)
        float L = MovementCalc(_acceleration, dt);

        // Move this distance on our Bezier curve (update the local variables of Position and Heading in directionScript)
        brainScript.directionScript.OnFrame(L, transform.position);

        // Position and heading updates
        Vector3 _position = brainScript.directionScript.Position;
        Vector3 _heading = brainScript.directionScript.Heading;

        UpdatePosition(_position);
        UpdateRotation(_heading);
        */

        
        float _oldLanePosition = LanePosition;

        LanePositionCalc = brainScript.directionScript.pathFunctions.ClosestPointOnPath(3, transform.position, 0.01f);
        LanePosition += (dt * carScriptableObject.Velocity);

        Debug.Log(Mathf.Abs(LanePosition - LanePositionCalc));
        MoveOnLane(LanePosition);
        


    }

    public void SpawnCar(int _carID, int _laneGiven)
    {

        // Update Variables
        CarID = _carID;
        Lane = _laneGiven;

        SpawnPositions();

        SpawnOther();

    }

    private void SpawnPositions()
    {

        // Set size, based on scriptable object assigned
        SetCarSize();

        // Get starting position
        LanePosition = PublicFunctions.SpawnPosition(laneManagerScript.LaneLengths[Lane], CarID);

        // Place in lane
        MoveOnLane(LanePosition);
    }

    private void SpawnOther()
    {
        // Spawn other scripts
        brainScript = new BrainScript();

        // Start ComputationCoroutine
        StartCoroutine(ComputationCoroutine());
    }

    private IEnumerator ComputationCoroutine()
    {

        WaitForSeconds _wait = new WaitForSeconds(RunSettings.COMPUTATION_DELAY);

        while (true)
        {
            yield return _wait;

            brainScript.RunBrain();

        }
    }

    private float MovementCalc(float _acceleration, float dt)
    {
        Velocity += _acceleration * dt;

        return (Velocity * dt) + (_acceleration * dt * dt / 2);
    }


    private void MoveOnLane(float _lanePosition)
    {

        UpdatePosition(laneManagerScript.LaneScriptArray[Lane].path.GetPointAtDistance(_lanePosition));
        UpdateRotation1(laneManagerScript.LaneScriptArray[Lane].path.GetRotationAtDistance(_lanePosition));

    }

    private void UpdatePosition(Vector3 _position)
    {
        transform.position = _position;

        Position = _position;
    }

    private void UpdateRotation(Vector3 _heading)
    {

        float rot_z = Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

        Heading = _heading;
    }

    private void UpdateRotation1(Quaternion RotationUpdate)
    {

        transform.rotation = RotationUpdate * Quaternion.Euler(0, -90, 0);

        Heading = transform.right;
    }

    private void SetCarSize()
    {
        // Calculate ratio between the box collider (fit to the image), and the final dimensions given in scriptable object
        float _lengthRatio = carScriptableObject.Length / boxCollider.size.x;
        float _widthRatio = carScriptableObject.Width / boxCollider.size.y;

        // Set car size via transform.scale
        transform.localScale = new Vector3(_lengthRatio, _widthRatio, 1);

    }
}
