using System.Collections;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    #region References and Awake
    // References
    public LaneManagerScript laneManagerScript { get; private set; }
    public CarScriptableObject carScriptableObject { get; private set; }
    public BoxCollider boxCollider { get; private set; }
    public BrainScript brainScript { get; private set; }
    public MovementScript movementScript { get; private set; }

    // Properties
    [field: SerializeField] public int CarID { get; private set; }
    [field: SerializeField] public int Lane { get; private set; }
    
    void Awake()
    {
        // References fill
        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();
        carScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/DefaultCar")) as CarScriptableObject;
        boxCollider = this.gameObject.GetComponent<BoxCollider>();

        // Spawn other scripts
        brainScript = new BrainScript();
        movementScript = new MovementScript(carScriptableObject.Velocity);
    }
    #endregion

    #region Run
    public void Run(float dt)
    {
        
        // Get acceleration
        float _acceleration = brainScript.accelerationScript.OnFrame(dt);

        // Calculate L (distance travelled in this frame)
        float L = movementScript.MovementCalc(_acceleration, dt);

        // Move this distance on our Bezier curve (update the local variables of Position and Heading in directionScript)
        brainScript.directionScript.OnFrame(L, dt);

        // Position and Heading updates
        UpdatePosition(brainScript.directionScript.Position);
        UpdateRotation(brainScript.directionScript.Heading);

    }
    #endregion

    #region Spawning
    public void SpawnCar(int _carID, int _laneGiven)
    {

        // Update Variables
        CarID = _carID;
        Lane = _laneGiven;

        // Set size, based on scriptable object assigned
        SetCarSize();

        // Set starting position in DirectionScript
        float _startLanePosition = PublicFunctions.SpawnPosition(laneManagerScript.LaneLengths[Lane], CarID);

        brainScript.directionScript.PutOnLane(Lane, _startLanePosition);

        // Start ComputationCoroutine
        StartCoroutine(BrainCoroutine());

    }

    private void SetCarSize()
    {
        // Calculate ratio between the box collider (fit to the image), and the final dimensions given in scriptable object
        float _lengthRatio = carScriptableObject.Length / boxCollider.size.x;
        float _widthRatio = carScriptableObject.Width / boxCollider.size.y;

        // Set car size via transform.scale
        transform.localScale = new Vector3(_lengthRatio, _widthRatio, 1);

    }
    #endregion

    #region Brain Coroutine
    private IEnumerator BrainCoroutine()
    {
        // On statup, pass spawn positions from DirectionScript
        brainScript.RunBrain(Lane, brainScript.directionScript.Position, brainScript.directionScript.Heading, movementScript.Velocity, true);

        WaitForSeconds _wait = new WaitForSeconds(RunSettings.COMPUTATION_DELAY);

        while (true)
        {
            yield return _wait;

            // Send REAL transform values, in case course correct is needed
            brainScript.RunBrain(Lane, transform.position, transform.right, movementScript.Velocity);

        }
    }
    #endregion

    #region Position + Rotation Updaters
    private void UpdatePosition(Vector3 _position)
    {
        transform.position = _position;
    }

    private void UpdateRotation(Vector3 _heading)
    {
     
        float rot_z = Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

    }
    #endregion

    
}
