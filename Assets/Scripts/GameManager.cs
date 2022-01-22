using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Controller RR;

    public GameObject needle;
    public Text gearNum;
    public Text kph;
    private float startPosizition = 32f, endPosition = 211f;
    private float desiredPosition;

    
    private void FixedUpdate()
    {
        kph.text = RR.KPH.ToString("0");
        updatedNeedle();
    }

    public void updatedNeedle()
    {
        desiredPosition = startPosizition - endPosition;
        float temp = RR.engineRPM / 10000;
        needle.transform.eulerAngles = new Vector3(0,0,(startPosizition - temp * desiredPosition));
    }

    public void changeGear()
    {
        gearNum.text = (!RR.reverse)? RR.gearNum.ToString() : "R";
    }

}
