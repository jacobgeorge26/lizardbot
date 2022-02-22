using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;

public class TrappedAlgorithm : MonoBehaviour
{
    private Queue<Vector3> locations = new Queue<Vector3>();
    private Queue<float> volumes = new Queue<float>();
    private GeneticAlgorithm AIScript;
    private RobotConfig robotConfig;

    //how many locations are analysed (2 per second)
    private int locationsSize = 20; 
    //these are used to prevent data being collected too soon 
    //the robot needs time to hit the terrain and react
    private bool ShowTrail = false;
    private bool ShowStuckPoints = false;

    private bool IsEnabled = true;

    private GameObject pointsContainer;

    //if the gradient of the variance of the magnitude of the coordinates dips below the limit then the robot is classed as stuck
    //this can be due to many different behaviours:
    //hitting a wall
    //circling
    //remaining in the same general area for too long
    //bouncing between the same locations
    void Start()
    {
        string name = "Stuck Points";
        bool containerCreated = false;
        foreach (Transform item in gameObject.transform.parent)
        {
            if (item.name == name) containerCreated = true;
        }
        if (!containerCreated)
        {
            pointsContainer = new GameObject();
            pointsContainer.name = name;
            pointsContainer.transform.parent = GetComponent<Transform>().parent;
        }
        AIScript = FindObjectOfType<GeneticAlgorithm>();
        ObjectConfig objConfig = this.gameObject.GetComponent<ObjectConfig>();
        robotConfig = AIConfig.RobotConfigs.Where(r => r.RobotIndex == objConfig.RobotIndex).First();

        StartCoroutine(IsTrapped());
    }

    internal void Reset()
    {
        IsEnabled = true;
        StartCoroutine(IsTrapped());
    }

    private IEnumerator IsTrapped()
    {
        while (IsEnabled)
        {
            yield return new WaitForSeconds(0.5f);
            if (robotConfig.IsEnabled) UpdateLocations();
        }
    }

    private void UpdateLocations()
    {
        //get 3D pythagoras of how far from the origin the robot has travelled
        Vector3 currentLocation = GetComponent<Transform>().position;
        //add this to locations
        locations.Enqueue(currentLocation);
        //locations is full, bring back to size then check if robot is stuck
        if (locations.Count > locationsSize)
        {
            int count = 100;
            //only store as many samples in locations as determined in AI config
            while (locations.Count > locationsSize && count > 0)
            {
                count--;
                locations.Dequeue();
            }
        }
        if(locations.Count == locationsSize)
        {
            float volume = GetVolume();
            //Grapher.Log(volume, "Volume", Color.white);
            volumes.Enqueue(volume);
            if(volumes.Count > locationsSize)
            {
                volumes.Dequeue();
            }
            if(volumes.Count == locationsSize)
            {
                float variance = GetVariance();
                //Grapher.Log(variance, "Variance", Color.red);
                if (Math.Round(variance) == 0)
                {
                    if (ShowStuckPoints)
                    {
                        GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
                        p.transform.position = currentLocation;
                        p.transform.parent = pointsContainer.transform;
                    }
                    IsEnabled = false;
                    AIScript.RobotIsStuck(robotConfig);        
                }
                else if(ShowTrail)
                {
                    GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
                    p.transform.position = currentLocation;
                    p.transform.parent = pointsContainer.transform;
                }
            }              

        }
        //important - update robot with its performance metric for AI to use
        robotConfig.Performance = currentLocation.magnitude > robotConfig.Performance ? currentLocation.magnitude : robotConfig.Performance;
    }

    private float GetVolume()
    {
        Vector3 min = locations.ElementAt(0), max = locations.ElementAt(0);
        for (int i = 0; i < locations.Count; i++)
        {
            Vector3 loc = locations.ElementAt(i);
            for (int axis = 0; axis < 3; axis++)
            {
                if (loc[axis] < min[axis]) min[axis] = loc[axis];
                if (loc[axis] > max[axis]) max[axis] = loc[axis];
            }
        }
        float largestDistance = 0;
        for (int axis = 0; axis < 3; axis++)
        {
            float distance = max[axis] - min[axis];
            largestDistance = distance > largestDistance ? distance : largestDistance;
        }
        return largestDistance * largestDistance * largestDistance;
    }

    private float GetVariance()
    {
        float mean = volumes.Sum() / volumes.Count();
        float[] squareMeanDiff = new float[volumes.Count];
        for (int i = 0; i < volumes.Count; i++)
        {
            //the division is just to avoid stupidly big numbers once squared
            float subMean = (volumes.ElementAt(i) - mean) / 100;
            squareMeanDiff[i] = subMean * subMean;
        }
        float variance = squareMeanDiff.Sum() / squareMeanDiff.Count();
        return variance;
    }
}
