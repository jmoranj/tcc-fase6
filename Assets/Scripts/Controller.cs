using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive 
    }
    [SerializeField]private driveType drive;

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
    private Rigidbody rigidbody;
    

    public float[] slip = new float[4];


    void Start()
    {
        getObjects();
    }

    private void FixedUpdate()
    {
        addDownForce();
        animateWheels();
        calculateEnginePower();
        steerVehicle();
        getFriction();
        shifter();
    }
    private void calculateEnginePower()
    {
        wheelRPM();

        totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum]) * IM.vertical;
        float velocity = 0.0f;
        engineRPM = Mathf.SmoothDamp(engineRPM, 1000 + (Mathf.Abs(wheelsRPM) * (gears[gearNum])), ref velocity, smoothTime);
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

        if(wheelsRPM < 0 && !reverse)
        {
            reverse = true;
            manager.changeGear();
        }
        else if(wheelsRPM > 0 && reverse)
        {
            reverse = false;
            manager.changeGear();
        }
    }

    private void shifter()
    {
        if (isGrounded()) return;

        if(gearChange == gearBox.automatic)
        {
            if(engineRPM > maxRPM && gearNum < gears.Length - 1 && !reverse)
            {
                gearNum++;
                manager.changeGear();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                gearNum++;
                manager.changeGear();
            }
            if (engineRPM < minRPM && gearNum > 0)
            {
                gearNum--;
                manager.changeGear();
            }
        }
        bool isGrounded()
        {
            if (wheels[0].isGrounded && wheels[1].isGrounded && wheels[2].isGrounded && wheels[3].isGrounded)
                return true;
            else
                return false;
        }
       
    }

    private void moveVehicle()
    {

        if(drive == driveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = totalPower / 4;
            }
        }else if(drive == driveType.rearWheelDrive)
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

        KPH = rigidbody.velocity.magnitude * 3.6f;

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
        else if(IM.horizontal < 0)
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

        for (int i = 0;i < 4; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }
    private void getObjects()
    {
        IM = GetComponent<InputManager>();
        rigidbody = GetComponent<Rigidbody>();
        centerOfMass = GameObject.Find("Mass");
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void addDownForce()
    {
        rigidbody.AddForce(-transform.up * DownForceValue * rigidbody.velocity.magnitude);
       
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