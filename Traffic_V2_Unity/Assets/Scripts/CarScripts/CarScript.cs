using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CarScript : MonoBehaviour
{
    // References
    private LaneManagerScript laneManagerScript; 
    private CarScriptableObject carScriptableObject;

    // Physical Properties
    private BoxCollider boxCollider;

    [field: SerializeField] public int CarID { get; private set; }
    [field: SerializeField] public int Lane { get; private set; }

    // Positional
    public Vector3 Position { get; private set; }
    public Vector3 Heading { get; private set; }



    // Start is called before the first frame update
    void Awake()
    {
        // References
        laneManagerScript = GameObject.FindGameObjectWithTag("LaneManager").GetComponent<LaneManagerScript>();
        carScriptableObject = ScriptableObject.Instantiate(Resources.Load("ScriptableObjects/CarScriptableObject")) as CarScriptableObject;

        // Phyiscal Properties
        boxCollider = this.gameObject.GetComponent<BoxCollider>();

    }

    public void SpawnCar(int _carID, int _laneGiven)
    {

        // Update Variables
        CarID = _carID;
        Lane = _laneGiven;

        // Set size, based on scriptable object assigned
        SetCarSize();

        // Place in lane
        MoveOnLane(SpawnPosition());

    }


    private float SpawnPosition()
    {
        float fraction = laneManagerScript.LaneLengths[Lane] / RunSettings.CARNUM;
        return fraction * CarID;
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
        float _colliderLength = boxCollider.size.x;
        float _colliderWidth = boxCollider.size.y;

        float _lengthRatio = carScriptableObject.Length / _colliderLength;
        float _widthRatio = carScriptableObject.Width / _colliderWidth;

        // Set collider
        // boxCollider.size = new Vector3(carScriptableObject.Length, carScriptableObject.Width, _colliderDepth);

        // Set visual
        transform.localScale = new Vector3(_lengthRatio, _widthRatio, 1);

    }
}
