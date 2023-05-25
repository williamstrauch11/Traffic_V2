using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;

public class FieldOfView : MonoBehaviour
{

    //TEMP-VISION EXPERIMENT
    /*
    public Vector3[] visionLineCars = new Vector3[5];
    public int visionCarNum;
    public int visionNum;
    */

    //References
    CarManagerScript carManagerScript;

    public int lookingStatus; // 0 = out of the front windshield, 1 = left blindspot check, 2 = right blindspot check, 3 = left mirror, 4 = right mirror, 5 = rearview mirror

    //public int[,] lookingArray; //6 looking status conditions, 9 cones
    //scan  min, mid, max. Vision min, mid, max. left, right, rearview.
    public int[,] lookingArray = new int[6, 9]
        {
        { 3, 3, 3, 2, 2, 2, 0, 0, 3}, //out of the front windshield
        { 3, 3, 0, 2, 2, 0, 3, 0, 0}, //left blindspot check
        { 3, 3, 0, 2, 2, 0, 0, 3, 0}, //right blindspot check
        { 3, 3, 0, 0, 0, 0, 2, 0, 0}, //left mirror
        { 3, 3, 0, 0, 0, 0, 0, 2, 0}, //right mirror
        { 3, 3, 0, 0, 0, 0, 0, 0, 2}, //rearviewmirror
        };

    public bool laneModified;

    List<int> newToLane = new List<int>();

    public bool laneEmpty;

    public float minDist;

    //The viewing system is comprised of many cones, of radius and angle. These cones have a heading (direction) and a position (origin)
    [HideInInspector]
    public float radiusScanCircle;

    [HideInInspector]
    public float radiusVisionMax, radiusVisionMid, radiusVisionMin, radiusScanMax, radiusScanMid, radiusScanMin; //Radiuss for forward views

    [HideInInspector]
    public float angleVisionMax, angleVisionMid, angleVisionMin, angleScanMax, angleScanMid, angleScanMin; //Angles for forward views

    [HideInInspector]
    public float radiusRearView, radiusSide; //Radiuss for mirror views

    [HideInInspector]
    public float angleRearView, angleSide; // Angles for mirror views

    [HideInInspector]
    public Vector3 carHeading, driverHeading, leftMirrorHeading, rearviewMirrorHeading, rightMirrorHeading; //Heading vectors

    [HideInInspector]
    public Vector3 driverVector, rearviewMirrorVector, leftMirrorVector, rightMirrorVector; //Offset vectors. Our view of cars in the rearview has an orgin in the mirror, not the driver. Drivers are not centered in the vehicle.

    [HideInInspector]
    public Vector3 carPosition, driverPosition, rearviewMirrorPosition, leftMirrorPosition, rightMirrorPosition; //Position vectors

    float[,] carArray = new float[constants.CarNum, 5];

    /*
    List<List<int>> numbers = new List<List<int>>(); // List of List<int>
    numbers.Add(new List<int>());  // Adding a new List<int> to the List.
    numbers[0].Add(2);  // Add the integer '2' to the List<int> at index '0' of numbers.

    
     * Row0 - Index
     * Row1 - Is In CircleScreen (1 = yes, 0 = no)
     * Row2 - Vision Degree (0 = blind, 1 = lookingat, 2 = in sight, 3 = periphery
     * Row3 - Lane
     * Row4 - Dist
     * Row5 - Position relative to car
     * Row6
     */

    public int lead_index = 0;
    public int lead_index_final = 0;

    public float death;

    public Vector3 targetPosition;

    //Mask for search
    public LayerMask carsMask;

    public bool carInView;

    // Start is called before the first frame update
    void Start()
    {

        //visionNum = 0;
        //Reference fill
        carManagerScript = GameObject.FindGameObjectWithTag("Manager").GetComponent<CarManagerScript>();

        laneEmpty = true;

        //looking status
        lookingStatus = 0; //0 = out of the front windshield, 1 = left blindspot check, 2 = right blindspot check, 3 = left mirror, 4 = right mirror, 5 = rearviewmirror

        //scan min, mid, max. Vision min, mid, max. left, right, rearview.
        //0 = out of the front windshield, 1 = left blindspot check, 2 = right blindspot check, 3 = left mirror, 4 = right mirror, 5 = rearviewmirror

        //FOR THE VISION PIES:
        //Radius Setting
        radiusVisionMax = 150f;
        radiusScanMax = radiusVisionMax;
        radiusVisionMid = radiusVisionMax / 2;
        radiusScanMid = radiusVisionMax * (2f / 3f);
        radiusVisionMin = radiusVisionMid / 3;
        radiusScanMin = radiusVisionMin;

        radiusScanCircle = (radiusVisionMax + radiusVisionMax / 20);

        radiusRearView = radiusVisionMid;
        radiusSide = radiusRearView;

        //Angle Setting
        angleScanMin = 180f;
        angleScanMid = angleScanMin / 2.5f;
        angleScanMax = angleScanMid / 2;
        angleVisionMin = angleScanMin / 2;
        angleVisionMid = angleVisionMin / 3;
        angleVisionMax = angleVisionMid / 2;

        angleRearView = 25f;
        angleSide = 20f;

        //Set offset vectors
        driverVector = new Vector3(-constants.CarWidth / 5, -constants.CarLength / 20f, 0f);
        rearviewMirrorVector = new Vector3(0f, 0f, 0f); //None for now, thought I'd build in the functionality
        leftMirrorVector = new Vector3(-constants.CarWidth / 2f, 0.6f, 0f);
        rightMirrorVector = new Vector3(constants.CarWidth / 2f, 0.6f, 0f);

        //Populate carArray
        for (int i = 0; i < constants.CarNum; i++)
        {
            carArray[i, 0] = i; //Set first row as index
            carArray[i, 1] = carArray[i, 2] = carArray[i, 4] = 0; //Set Row 1 and 2 to 0
            carArray[i, 3] = 3;
        }

        StartCoroutine(FOVroutine());

    }

    private IEnumerator FOVroutine()
    {
        float delay = 0.1f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }

    }

    private void FieldOfViewCheck()
    {
        //TEMP-VISION EXPERIMENT
        //visionCarNum = 0;

        //View Updates
        //Heading updates:
        carHeading = transform.right;
        driverHeading = GetComponent<CarScript>().visionDirection; //This points out of the front of the car. TO CHANGE
        rearviewMirrorHeading = Quaternion.AngleAxis(180f, Vector3.forward) * carHeading; //Rotate carHeading 180 degrees
        leftMirrorHeading = Quaternion.AngleAxis(180f - (angleSide / 2), Vector3.forward) * carHeading; //Rotate carHeading vector 180 degrees to point directly back, then rotate it angle//2 degrees to have one side see straight back
        rightMirrorHeading = Quaternion.AngleAxis(180f + (angleSide / 2), Vector3.forward) * carHeading; //Same as left, dif sign
        
        //Position updates
        carPosition = transform.position;
        driverPosition = constants.OffsetCalc(carHeading, carPosition, driverVector);
        rearviewMirrorPosition = constants.OffsetCalc(carHeading, carPosition, rearviewMirrorVector);
        leftMirrorPosition = constants.OffsetCalc(carHeading, carPosition, leftMirrorVector);
        rightMirrorPosition = constants.OffsetCalc(carHeading, carPosition, rightMirrorVector);

        //Do one check with the initial scan circle first 
        Collider[] rangeChecks = Physics.OverlapSphere(carPosition, radiusScanCircle, carsMask);

        laneModified = false;

        int length = rangeChecks.Length;

        for (int i = 0; i < length; i++)
        {
            int getID = rangeChecks[i].GetComponent<CarScript>().CarID;
            carArray[getID, 1] = 1; // In CicleScreen
            carArray[getID, 2] = 0; //reset VisionCheck

            if (GetComponent<CarScript>().CarID != getID) //exclude self
            {

                //Scan Cone Checks
                ConeCheck(driverPosition, radiusScanMin, angleScanMin, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 0], getID); //Position, radius, angle, heading, collider being searched, VisionDegree, index
                ConeCheck(driverPosition, radiusScanMid, angleScanMid, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 1], getID);
                ConeCheck(driverPosition, radiusScanMax, angleScanMax, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 2], getID);

                //Vision Cone Checks
                ConeCheck(driverPosition, radiusVisionMin, angleVisionMin, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 3], getID);
                ConeCheck(driverPosition, radiusVisionMid, angleVisionMid, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 4], getID);
                ConeCheck(driverPosition, radiusVisionMax, angleVisionMax, driverHeading, rangeChecks[i], lookingArray[lookingStatus, 5], getID);

                //SideMirrors
                ConeCheck(leftMirrorPosition, radiusSide, angleSide, leftMirrorHeading, rangeChecks[i], lookingArray[lookingStatus, 6], getID);
                ConeCheck(rightMirrorPosition, radiusSide, angleSide, rightMirrorHeading, rangeChecks[i], lookingArray[lookingStatus, 7], getID);

                //RearViewMirror
                ConeCheck(rearviewMirrorPosition, radiusRearView, angleRearView, rearviewMirrorHeading, rangeChecks[i], lookingArray[lookingStatus, 8], getID);
            }
        }

        //Update Lane
        if (laneModified)
        {
            for (int i = 0; i < newToLane.Count; i++)
            {
                if (!laneEmpty)
                {
                    if (carArray[newToLane[i], 4] < carArray[lead_index, 4])
                    {
                        lead_index = newToLane[i];
                    }
                }
                else
                {
                    lead_index = newToLane[i];

                    laneEmpty = false;
                }
            }
        }

        if (!laneEmpty)
        {
            carInView = true;
            lead_index_final = lead_index;
            minDist = carArray[lead_index, 4];
            targetPosition = carManagerScript.cars_object[lead_index_final].transform.position;

        }
        else
        {
            carInView = false;
        }

        //Empty newToLane
        newToLane.Clear();

        
    }

    void ConeCheck(Vector3 position, float radius, float angle, Vector3 heading, Collider searchCollider, float visionDegree, int id)
    {
        
        if (visionDegree == 0) // Don't bother running, if this is a blindspot anyway
        {
            return;
        }

        //Use 'target' for transform functions of current target car
        Transform target = searchCollider.transform;

        //Get direction to the car, to be used for cone check
        Vector3 directionToTarget = (target.position - position).normalized;

        
        //Cone check
        if (Vector3.Angle(heading, directionToTarget) < angle / 2)
        {
            //If passed, find the distance to target car
            float dist = Vector3.Distance(target.position, position);

            if (dist < radius)
            {

                if (carArray[id, 2] == 0 || carArray[id, 2] > visionDegree)
                {

                    carArray[id, 2] = visionDegree;
                     
                    if (visionDegree == 2)
                    {
                        /*
                        RaycastHit[] hits = Physics.RaycastAll(position, directionToTarget, dist, carsMask);

                        if (hits.Length < 2)
                        {
                            visionLineCars[visionCarNum] = target.position;
                            visionCarNum++;
                        }
                        */

                        if (carArray[id, 3] != searchCollider.GetComponent<CarScript>().lane)
                        {

                            carArray[id, 3] = searchCollider.GetComponent<CarScript>().lane;

                            if (carArray[id, 3] == GetComponent<CarScript>().lane)
                            {
                                
                                newToLane.Add(id);
                                laneModified = true;

                            }

                        }

                        carArray[id, 4] = dist;
                    }

                }
            }
        }
    }
}

    /*
     * Row0 - Index
     * Row1 - Is In CircleScreen (1 = yes, 0 = no)
     * Row2 - Vision Degree (0 = blind, 1 = lookingat, 2 = in sight, 3 = periphery
     * Row3 - Lane
     * Row4 - Dist
     * Row5
     * Row6
     */