using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    internal enum driver
    {
        keyboard,
        AI
    }
    [SerializeField] driver driveContreller;

    public float vertical;
    public float horizontal;
    public bool handBrake;
    internal bool boosting;

    public TrackWayPoints wayPoints;
    public Transform currentWayPoint;
    public List<Transform> nodes = new List<Transform>();
    [Range(0, 10)] public int distanceOffset;
    [Range(0, 5)] public float sterrForce;

    private void Awake()
    {
        wayPoints = GameObject.FindGameObjectWithTag("Path").GetComponent<TrackWayPoints>();

        nodes = wayPoints.nodes;
    }
    private void FixedUpdate()
    {
        switch (driveContreller)
        {
            case driver.AI:
                AiDrive();
                break;
            case driver.keyboard:
                KeyboardDrive();
                break;

        }
        calculateDistanceOfWayPoints();
    }

    private void AiDrive()
    {
        vertical = .3f;
        AISteer();
    }

    private void KeyboardDrive()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handBrake = (Input.GetAxis("HandBrake") != 0) ? true : false;
    }

    private void calculateDistanceOfWayPoints()
    {
        Vector3 position = gameObject.transform.position;
        float distance = Mathf.Infinity;

        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 difference = nodes[i].transform.position - position;
            float currentDistance = difference.magnitude;
            if (currentDistance < distance)
            {
                currentWayPoint = nodes[i + distanceOffset];
                distance = currentDistance;
            }
        }


    }

    private void AISteer()
    {
        Vector3 relative = transform.InverseTransformPoint(currentWayPoint.transform.position);
        relative /= relative.magnitude;

        horizontal = (relative.x / relative.magnitude) * sterrForce;
    }
    private void OnDrawGiszmos()
    {
        Gizmos.DrawWireSphere(currentWayPoint.position, 3);
    }
}
