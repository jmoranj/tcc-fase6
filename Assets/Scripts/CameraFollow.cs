using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject Player;
    private Controller RR;
    private GameObject cameraConstarint;
    private GameObject cameraLookAt;
    public float speed = 0;
    public float defaultFOV = 0,desiredFOV = 0;
    [Range(0, 5)] public float smothTime = 0;
    
    public void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        cameraConstarint = Player.transform.Find("Camera onstraint").gameObject;
        cameraLookAt = Player.transform.Find("camera LookAt").gameObject;
        RR = Player.GetComponent<Controller>();
        defaultFOV = Camera.main.fieldOfView;
    }

    private void FixedUpdate()
    {
        follow();

        speed = (RR.KPH >= 50) ? 20 : RR.KPH / 4;
    }
    private void follow()
    {
        if (speed <= 23)
            speed = Mathf.Lerp(speed, RR.KPH / 2, Time.deltaTime);
        else
            speed = 23;

        gameObject.transform.position = Vector3.Lerp(transform.position, cameraConstarint.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(cameraLookAt.gameObject.transform.position);
    }
    private void boostFOV()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, desiredFOV, Time.deltaTime * smothTime);
        else
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defaultFOV, Time.deltaTime * smothTime);

    }
}
