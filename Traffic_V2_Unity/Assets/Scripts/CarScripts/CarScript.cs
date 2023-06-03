using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    public float LanePosition { get; private set; } 



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



        // Movement, find
        // Movement, execute



        float _oldLanePosition = LanePosition;

        LanePosition += (dt * carScriptableObject.Velocity);
        MoveOnLane(LanePosition);


    }

    public void SpawnCar(int _carID, int _laneGiven)
    {

        // Update Variables
        CarID = _carID;
        Lane = _laneGiven;

        // Set size, based on scriptable object assigned
        SetCarSize();

        // Get starting position
        LanePosition = PublicFunctions.SpawnPosition(laneManagerScript.LaneLengths[Lane], CarID);

        // Place in lane
        MoveOnLane(LanePosition);

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
            Debug.Log("hi");
            brainScript.RunBrain();
        }
    }


    private void MoveOnLane(float _lanePosition)
    {

        UpdatePosition(laneManagerScript.LaneScriptArray[Lane].path.GetPointAtDistance(_lanePosition));
        UpdateRotation(laneManagerScript.LaneScriptArray[Lane].path.GetRotationAtDistance(_lanePosition));

    }

    private void UpdatePosition(Vector3 PositionUpdate)
    {
        transform.position = PositionUpdate;

        Position = PositionUpdate;
    }

    private void UpdateRotation(Quaternion RotationUpdate)
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
