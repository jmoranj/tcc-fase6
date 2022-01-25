using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Controller : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField] private driveType drive;

    internal enum gearBox
    {
        automatic,
        manual
    }
    [SerializeField] private gearBox gearChange;

    public GameManager manager;

    [Header("Variables")]
    public float totalPower;
    public float maxRPM, minRPM;
    public float KPH;
    public float wheelsRPM;
    public float engineRPM;
    public bool reverse = false;
    public float smoothTime = 0.01f;
    public float[] gears;
    public int gearNum = 0;
    public float brakePower;
    public float radius = 6;
    public float DownForceValue = 50;
    public float motortorque = 200;
    public float steeringMax = 4;
    public AnimationCurve enginePower;

    private InputManager IM;
    public WheelCollider[] wheels = new WheelCollider[4];
    public GameObject[] wheelMesh = new GameObject[4];
    public GameObject centerOfMass;
    private Rigidbody rigidBody;


    public float[] slip = new float[4];


    void Start()
    {
        getObjects();
    }

    private void FixedUpdate()
    {
        getFriction();
        animateWheels();
        addDownForce();
        calculateEnginePower();
        steerVehicle();
        shifter();
    }
    private void calculateEnginePower()
    {
        wheelRPM();

        totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * IM.vertical;
        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * 3.6f * (gears[gearNum])), ref velocity, smoothTime);

        moveVehicle();
    }
    private void wheelRPM()
    {
        float sum = 0;
        int R = 0;

        for (int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
    }

    private void shifter()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Console.WriteLine(gearNum);
            gearNum++;
            manager.changeGear();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            gearNum--;
            manager.changeGear();
        }

    }

    private void moveVehicle()
    {

        if (drive == driveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = totalPower / 4;
            }
        }
        else if (drive == driveType.rearWheelDrive)
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = IM.vertical * (totalPower / 2);
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length - 2; i++)
            {
                wheels[i].motorTorque = (totalPower / 2);
            }
        }

        KPH = rigidBody.velocity.magnitude * 3.6f;
        if (IM.handBrake)
        {
            wheels[3].brakeTorque = wheels[2].brakeTorque = brakePower;
        }
        else
        {
            wheels[3].brakeTorque = wheels[2].brakeTorque = 0;

        }
    }

    private void steerVehicle()
    {
        if (IM.horizontal > 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
        }
        else if (IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }
    }

    private void animateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < 4; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }
    private void getObjects()
    {
        IM = GetComponent<InputManager>();
        rigidBody = GetComponent<Rigidbody>();
        centerOfMass = GameObject.Find("Mass");
        rigidBody.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void addDownForce()
    {
        rigidBody.AddForce(-transform.up * DownForceValue * rigidBody.velocity.magnitude);

    }

    private void getFriction()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelHit wheelHit;
            wheels[i].GetGroundHit(out wheelHit);

            slip[i] = wheelHit.forwardSlip;
        }
    }
}