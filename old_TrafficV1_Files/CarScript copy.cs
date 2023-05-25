using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PathCreation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class CarScript : MonoBehaviour
{

    CarManagerScript carManagerScript;
    SpriteRenderer spriteRenderer;

    
    [SerializeField] Sprite[] spriteArray; //[0]nobrake, [1]brake
    public int CarID;

    //Lane
    public int lane;
    PathCreator[] pathCreators; //Array for lanes
    private int laneNumber;
    private float[] laneLengths; //Array of lane lengths

    //LaneChanging
    private bool laneSwitch; //true while switch is happening
    public float aggressivness; //Between 0.8 and 1.3 work 
    [HideInInspector] public Vector3[] nodeArray = new Vector3[constants.nodeNum];

    //Motion Variables
    private float position;
    private float velocity;
    public float acceleration;
    [HideInInspector] private float M;
    private float coastAccel;

    //VisionVaribales
    [HideInInspector] public Vector3 visionDirection;
    public bool followingCar;

    //Other Motion Variables
    private float maxVelocity;
    private float maxAcceleration;
    private float maxBrake;
    private bool active; //Used for manual stopping and starting of car with mouse
    private bool stopped; //When the car is physically not moving
    private bool brake; //Used for braking visuals

    //AccelerationChange
    private int accelArrayNum;
    private float[] previousAccelerations;
    private float accelChangeTime;
    [SerializeField] float accelerationOffset;
    [SerializeField] float a2;
    [HideInInspector] public float accelerationChange;
    [HideInInspector] public float accelerationChangeAdded;
    private float accelerationFractionAdded;
    private int accelArrayCounter;
    private float reactionTime;
    [HideInInspector] public float accelChangeInTime;


    //Motion Calculation Variables
    private float L; //More about these in the calculation code
    private float followSensitivity;
    private float slopeScale;

    //First things first
    private void Awake()
    {
        //fill the classes in the interface
        carManagerScript = GameObject.FindGameObjectWithTag("Manager").GetComponent<CarManagerScript>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        //Initialize bools
        stopped = false;
        laneSwitch = false;
        active = true;

        //Lanes
        pathCreators = GameObject.FindGameObjectWithTag("LaneManager").GetComponentsInChildren<PathCreator>(); //Fill out array for lanes
        laneNumber = carManagerScript.laneNumber; //Get number of lanes

        //Find and set array of lane lengths
        laneLengths = new float[(int)laneNumber];

        for (int i = 0; i < laneNumber; i++)
            laneLengths[i] = pathCreators[i].path.length;
    }

    //Before first frame
    void Start()
    {
        
        accelArrayNum = 10;
        accelChangeTime = 0.1f; //seconds
        previousAccelerations = new float[accelArrayNum];
        accelerationFractionAdded = 0;
        accelArrayCounter = 0;
        reactionTime = 0.5f;
        accelChangeInTime = 0f;
        
        for (int i = 0; i < accelArrayNum; i++)
        {
            previousAccelerations[i] = 0f;
        }


        followingCar = false;

        //LaneChanging
        aggressivness = 90f;

        //Motion variables
        maxVelocity = 47f; //46.93 is 60 mph
        maxAcceleration = 6.2f; //7.5 seconds 0-60 = 6.2f
        maxBrake = Random.Range(-9f, -8f); //140-120 ft stopping distance at 60 mph (-9 , -8)
        accelerationOffset = 0;

        coastAccel = -0.8f; //below, braking!

        acceleration = 0;

        brake = false;

        //set nobrake sprite
        ChangeSprite(0);

    }

    void OnMouseEnter()
    {
        //Brake
        acceleration = maxBrake;
        active = false;
    }

    private void OnMouseDown()
    {
        //Stop braking
        //active = true;
        acceleration = 0;
    }


    // Update is called once per frame
    void Update()
    {
        //Brake visuals
        if (acceleration >= coastAccel)
        {
            if (brake)
            {
                brake = false;
                ChangeSprite(0);
            }

        }
        else
        {
            if (!brake)
            {
                brake = true;
                ChangeSprite(1);
            }
        }

        //Vision Direction Set
        if (GetComponent<FieldOfView>().carInView)
        {
            int lead = GetComponent<FieldOfView>().lead_index;
            CarScript target = carManagerScript.cars_scripts[lead];
            visionDirection = (target.transform.position - transform.position).normalized;
        }
        else 
        {
            float sensitivity = 2f;
            float idealLaneScanPosition = (position + (velocity * sensitivity)) % laneLengths[lane];
            Vector3 target = pathCreators[lane].path.GetPointAtDistance(idealLaneScanPosition);
            visionDirection = (target - transform.position).normalized;
        }

        

        //Check for lane change key
        if (Input.GetKeyDown("left"))
        {
            LandChangeInitiate(1);
        }

        if (Input.GetKeyDown("right"))
        {
            LandChangeInitiate(0);
        }

        if (CarID == 2)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                acceleration = maxBrake/2;
                active = false;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                active = true;

            }
        }
        
    }

    //Called once per frame by carManagerScript, to keep things centralized
    internal void Run(int lead_int, float dt)
    {

        CarScript leadScript = carManagerScript.cars_scripts[lead_int];
        Transform leadTransform = carManagerScript.cars_object[lead_int].transform;

        //Update position
        if (!stopped) //If stopped, no change in lane
        {

            if (!laneSwitch)
            {
                position += L;

                position %= laneLengths[lane];

                //Update position
                transform.position = pathCreators[lane].path.GetPointAtDistance(position);

                //Update rotation, apply y-axis offset
                Quaternion originalRot = pathCreators[lane].path.GetRotationAtDistance(position);
                transform.rotation = originalRot * Quaternion.Euler(0, -90, 0);

            }
            //If switching lanes
            else
            {
                
                transform.position = gameObject.GetComponent<LaneChanger>().GetPosition(transform.position, L);

                Vector3 heading = gameObject.GetComponent<LaneChanger>().heading;

                float rot_z = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

                laneSwitch = gameObject.GetComponent<LaneChanger>().reset;
            }

        }

        float initialAcceleration = acceleration;

        //Get info about car in front
        bool carInView = gameObject.GetComponent<FieldOfView>().carInView;

        //Calculate new acceleration based off of lead car
        if (active && carInView)
        {
   
            Vector3 carFront = constants.OffsetCalc(transform.right, transform.position, constants.topMiddle);
            Vector3 leadBack = constants.OffsetCalc(leadTransform.right, leadTransform.position, constants.backMiddle);

            float delta_position = -Vector3.Distance(carFront, leadBack);


            //Acceleration offset calculation (this raises and lowers the curve, so that we maintain our follow distance even during accelerations
            //More to be worked on here
            float smoothTime = 0.02f;
            float yVelocity = 0.0f;

            a2 = leadScript.acceleration;


            if (a2 > accelerationOffset) //accelerating, we are less responsive
            {
                smoothTime = 0.06f;
            }

            if (Mathf.Abs(leadScript.accelChangeInTime) <= 40)
            {
                accelerationOffset = Mathf.SmoothDamp(accelerationOffset, a2, ref yVelocity, smoothTime);
            }

            if (CarID == 28)
            {
                Debug.Log(leadScript.accelChangeInTime);
            }

            float v1 = velocity;
            float v2 = leadScript.velocity;

            float velocitySensitivity = 5f;

            float result = 0;


            //https://www.desmos.com/calculator/vyo3ghtgat
            //FollowDistance
            M = 0.7f + 0.8f * v2 - 0.005f * Mathf.Pow(v2, 2); //positive

            float accelSlope = 0.18f;
            float brakeSlopeMax = 9f;

            float maxFollowDistance = 32.7f;
            float velocityAtMaxFollowDistance = 80;

            if (v2 > velocityAtMaxFollowDistance)
            {
                M = maxFollowDistance;
            }

            //Curve for when v1 < v2 (Kind of - We keep a bit of the linear function past this point to smoothly transition into the quartercircle function)
            float F = (0.7f / 20f) * v2 + 1f; //F = minimum follow distance, positive 
            float E = 1 - (v1 - v2) / v2;
            float C;
            if (E >= 1)
            {
                C = 1;
            }
            else
            {
                C = Mathf.Sqrt(1 - Mathf.Pow(E, 2));
            }

            float H = (M - F) * (1 - C) + F;

            //Curve when v2 > v1
            float G = (v1 - v2) * velocitySensitivity;
            float I = G + M;
            
            if (I <= H)
            {
                result = AccelCalculator(H, delta_position, accelSlope, brakeSlopeMax, F);
            }
            else
            {
                result = AccelCalculator(I, delta_position, accelSlope, brakeSlopeMax, F);
            }

            if (result > maxAcceleration)
            {
                acceleration = maxAcceleration;
            }
            else if (result < maxBrake)
            {
                acceleration = maxBrake;
            }
            else
            {
                acceleration = result;
            }

        }
        else if (active && !carInView)
        {
            acceleration = maxAcceleration;
        }

        //Calculate new position and velocity from acceleration
        //If we are stopped
        if ((velocity + acceleration * dt) <= 0)
        {
            position -= 0.5f * velocity * (velocity / acceleration);
            velocity = 0;
            stopped = true;

            //we don't touch position
        }
        else
        {

            if ((velocity + acceleration * dt) > maxVelocity)
            {
                acceleration = 0;
            }

            velocity += acceleration * dt;

            L = velocity * dt + acceleration * dt * dt / 2;

            if (stopped)
                stopped = false;

        }

        //Acceleration Change over the last (accelChangeTime) seconds
        accelerationChange = (acceleration - initialAcceleration);

        float accelerationChangeAdjusted = accelerationChange / dt;

        float timeFraction = (accelChangeTime / accelArrayNum) / dt;

        accelerationFractionAdded += timeFraction; 

        accelerationChangeAdded += accelerationChangeAdjusted;

        if (accelerationFractionAdded > 1)
        {

            previousAccelerations[accelArrayCounter] = accelerationChangeAdded;
            
            accelerationChangeAdded = 0;
            accelerationFractionAdded -= 1;

            if (accelArrayCounter < (accelArrayNum - 1))
            {
                accelArrayCounter++;
            }
            else
            {
                accelArrayCounter = 0;
            }

        }

        accelChangeInTime = constants.SumArray(previousAccelerations);

    }

    internal void placeCar(int given_id, int lane_given)
    {

        lane = lane_given;
        velocity = constants.SpawnSpeed;
        CarID = given_id;

        //set position
        float fraction = laneLengths[lane] / constants.CarNum;
        position = fraction * CarID;

    }

    float ProximitySearch(int search_index, Vector3 position)
    {

        float range = laneLengths[search_index];

        int run_num = 12;

        int run = 1;

        float guess = 0f;

        for (int i = 0; i < run_num; i++)
        {
         
            float top = (guess + range / (2 * Mathf.Pow(2, run))) % range;
            float bottom = ((guess + range) - range / (2 * Mathf.Pow(2, run))) % range;
      
            float resultTop = Distance(top, position, search_index);

            float resultBottom = Distance(bottom, position, search_index);

            if (resultTop < resultBottom)
            {
                guess = top;
            }
            else
            {
                guess = bottom;
            }

            run++;
        }

        return guess;
    }

    float Distance(float search_position, Vector3 car_position, int search_index)
    {
        Vector3 searchPointTop = pathCreators[search_index].path.GetPointAtDistance(search_position);

        float result;

        return result  = Vector3.Distance(car_position, searchPointTop);
    }

    private void SetLaneChange(int laneStart, int laneEnd, float closePointOnEndLane) 
    {

        //First node is our position
        nodeArray[0] = pathCreators[laneStart].path.GetPointAtDistance(position);
        Vector3 startDirection = pathCreators[laneStart].path.GetDirectionAtDistance(position);

        //use max to look for angle in calc. These values work well for this track
        float straightTravelDistance = 120f;
        float turnTravelDistance = aggressivness;

        float calcDistance = (straightTravelDistance + turnTravelDistance) / 2;
        float tempTargetPosition = closePointOnEndLane + calcDistance; 

        Vector3 tempEndDirection = pathCreators[laneEnd].path.GetDirectionAtDistance(tempTargetPosition);

        float angleBetween = Vector3.Angle(startDirection, tempEndDirection); //This is the angle between the start and (temp)end vectors. We use this to see how much of a turn we are on

        //100 on straight @ 0 degrees
        //75 on turn @ 60 degrees

        float maxTurnAngle = 60f; //measured, but its not entirely accurate. Basically, all of these parameters are subject to change. 

        float normalizedAngle = angleBetween / maxTurnAngle;

        //float travelDistance = Mathf.Lerp(straightTravelDistance, turnTravelDistance, normalizedAngle) / aggressivness; //Length
        float travelDistance = turnTravelDistance;

        float targetPosition = closePointOnEndLane + travelDistance;

        position = targetPosition;

        nodeArray[3] = pathCreators[laneEnd].path.GetPointAtDistance(targetPosition);
        Vector3 endDirection = pathCreators[laneEnd].path.GetDirectionAtDistance(targetPosition);

        //0.1 on straight @ 0 degree
        //0.25 on turn @ 60 degrees

        float straightNodeFraction = 0.1f;
        float turnNodeFraction = 0.3f;

        float nodeFraction = Mathf.Lerp(straightNodeFraction, turnNodeFraction, normalizedAngle);


        nodeArray[1] = (startDirection * (travelDistance * nodeFraction)) + nodeArray[0];
        nodeArray[2] = ((endDirection * -1) * (travelDistance * nodeFraction)) + nodeArray[3];

    }

    void ChangeSprite(int spriteIndex)
    {
        spriteRenderer.sprite = spriteArray[spriteIndex];
    }

    void LandChangeInitiate(int RL)
    {
        

        if (!laneSwitch)
        {
            int newlane = lane;
            bool switchPossible = true;

            if (RL == 1)
            {
                if ((lane + 1) >= laneNumber)
                {
                    switchPossible = false;
                }
                else
                {
                    newlane += 1;
                }
            }
            else if (RL == 0)
            {
                if ((lane - 1) < 0)
                {
                    switchPossible = false;
                }
                else
                {
                    newlane -= 1;
                }
            }

            if (switchPossible)
            {
                //RL = 0, L. RL = 1, R
                laneSwitch = true; //This bool means that we are currently in a lane switch

                float nextLanePos = ProximitySearch(newlane, transform.position); //Returns the distance value for the point on the other lane closest to transform.position

                SetLaneChange(lane, newlane, nextLanePos); //This populates nodeArray with the correct values. The LaneChanger script will use these for its calculations

                lane = newlane;
            }
            
        }

    }
    private float AccelCalculator(float J, float delta_position, float accelSlope, float brakeSlopeMax, float F)
    {
        float tempY;

        tempY = (delta_position + J) + accelerationOffset;

        float distanceAtZero = -((tempY - accelerationOffset) - J); //positive

        float slope;

        if (delta_position <= distanceAtZero)
        {
            slope = -accelSlope;
        }
        else
        {
            //linear exrapolation between accel slope when the 0 is at M, and maxbrake when the 0 is at F
            float diff = M - F;

            float frac = (distanceAtZero - F) / diff;

            slope = -(((brakeSlopeMax - accelSlope) * frac) + brakeSlopeMax);

        }

        float resultLocal;

        resultLocal = slope * (delta_position + J) + accelerationOffset;

        return resultLocal;
    }
}