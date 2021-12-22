using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;

public class SuccessorFunction : MonoBehaviour
{
    Queue<float> locations = new Queue<float>();

    void Start()
    {
        float magnitude = GetComponent<Transform>().position.magnitude;
        for (int i = 0; i < AIConfig.SuccessorCache; i++)
        {
            locations.Enqueue(magnitude);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLocations();
        DetermineRobotStuck();
    }

    private void DetermineRobotStuck()
    {
        Debug.Log(locations.Max() - locations.Min());
        //if the average from the last x frames is under 1 then the robot is bouncing and is stuck
        if(locations.Max() - locations.Min() < 0.5)
        {
            Debug.LogError("Robot Stuck");
        }
    }

    private void UpdateLocations()
    {
        //get 3D pythagoras of how far from the origin the robot has travelled
        Vector3 currentLocation = GetComponent<Transform>().position;
        float magnitude2D = Mathf.Sqrt((currentLocation.x * currentLocation.x) + (currentLocation.z * currentLocation.z));
        float magnitude3D = Mathf.Sqrt(magnitude2D + (currentLocation.y * currentLocation.y));
        locations.Enqueue(magnitude3D);
        //only store as many as determined in AI config
        if (locations.Count > AIConfig.SuccessorCache)
        {
            locations.Dequeue();
        }
    }
}
