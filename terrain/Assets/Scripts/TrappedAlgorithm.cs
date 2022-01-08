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

    //these are used to determine how many frames pass before the location is analysed
    private int sampleCount = 0; //starts at 0, will be assigned framerate / 2
    private int locationsSize = 20; 
    //these are used to prevent data being collected too soon 
    //the robot needs time to hit the terrain and react
    private bool IsEnabled = false;
    private int StartBuffer = 100;
    private bool ShowTrail = false;

    private GameObject pointsContainer;

    private void Start()
    {
        pointsContainer = new GameObject();  
    }

    //if the gradient of the variance of the magnitude of the coordinates dips below the limit then the robot is classed as stuck
    //this can be due to many different behaviours:
    //hitting a wall
    //circling
    //remaining in the same general area for too long
    //bouncing between the same locations
    void Update()
    {
        //variance on init can take a few frames to start returning a proper result
        StartBuffer -= IsEnabled ? 1 : 0;
        if(StartBuffer <= 0)
        {
            if(sampleCount <= 0)
            {
                float framerate = 1.0f / Time.deltaTime;
                sampleCount = (int)framerate / 2;
                UpdateLocations();
            }
            else
            {
                sampleCount--;
            }
            
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
            volumes.Enqueue(volume);
            if(volumes.Count > locationsSize)
            {
                volumes.Dequeue();
            }
            if(volumes.Count == locationsSize)
            {
                float variance = GetVariance();

                if (Math.Round(variance) == 0)
                {
                    GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
                    p.transform.position = currentLocation;
                    p.transform.parent = pointsContainer.transform;

                }
                else if(ShowTrail)
                {
                    GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
                    p.transform.position = currentLocation;
                    p.transform.parent = pointsContainer.transform;
                }
            }              

        }
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

    internal void Enable()
    {
        IsEnabled = true;
    }
}
