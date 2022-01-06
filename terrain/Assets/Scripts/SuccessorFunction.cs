using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;

public class SuccessorFunction : MonoBehaviour
{
    private Queue<float> locations = new Queue<float>();
    private Queue<float> tempLocations = new Queue<float>();

    private int frameRate;
    private int sampleSize = 50;
    private float searchSize = 3f;
    private float limit;
    private bool IsEnabled = false;
    private bool CountIsStuck = false;

    void Start()
    {
        //the limit is used to determine when the robot is stuck
        //if the variance dips below the limit then the robot is classed as stuck
        //this can be due to many different behaviours:
        //hitting a wall
        //circling
        //remaining in the same general area for too long
        //bouncing between the same locations
        //this was based on data collection - see data collection folder for the actual data
        limit = TerrainConfig.SurfaceType switch
        {
            Surface.Smooth => 0.062f,
            Surface.Uneven => 0.023f,
            Surface.Rough => 0.432f,
            _ => throw new NotImplementedException()
        };
    }

    void Update()
    {
        if(IsEnabled)
        {
            frameRate = ((int)(1.0f / Time.deltaTime));
            UpdateLocations();
        }
    }

    internal void Enable()
    {
        IsEnabled = true;
    }

    private float GetVariance()
    {
        float mean = locations.Sum() / locations.Count();
        float[] squareMeanDiff = new float[locations.Count];
        for (int i = 0; i < locations.Count; i++)
        {
            float subMean = locations.ElementAt(i) - mean;
            squareMeanDiff[i] = subMean * subMean;
        }
        float variance = squareMeanDiff.Sum() / squareMeanDiff.Count();
        return variance;
        //variance on init can take a few frames to start returning a result
    }
    
    private int GetLocationsLimit()
    {
        //the number of samples collected is based on the frame rate - to roughly equate to the number of seconds. 
        //e.g. we want the last ~second to be analysed and the framerate is 300. 1 sample = 50 frames. 
        //2 values are added to locations each sample
        //the max size of locations should be 12
        //the minimum value will always be 10 to accomodate lower frame rate
        return 15;
        return Math.Max((int)((frameRate * 2 * searchSize) / sampleSize), 15);
    }

    private void UpdateLocations()
    {
        //get 3D pythagoras of how far from the origin the robot has travelled
        Vector3 currentLocation = GetComponent<Transform>().position;
        float magnitude2D = Mathf.Sqrt((currentLocation.x * currentLocation.x) + (currentLocation.z * currentLocation.z));
        float magnitude3D = Mathf.Sqrt(magnitude2D + (currentLocation.y * currentLocation.y));
        //add this to the current sample being collected
        tempLocations.Enqueue(magnitude3D);
        ///////////testing
        Grapher.Log(magnitude3D, "Location", Color.red);
        //GameObject locPoint = MonoBehaviour.Instantiate(Resources.Load<GameObject>("LocationPoint"));
        //locPoint.transform.position = currentLocation;
        if (tempLocations.Count >= sampleSize)
        {
            //this sample is complete - store the max and min of this sample in locations
            locations.Enqueue(tempLocations.Max());
            locations.Enqueue(tempLocations.Min());
            tempLocations.Clear();
        }
        int count = 100;
        //only store as many samples in locations as determined in AI config
        while(locations.Count > GetLocationsLimit() && count > 0)
        {
            count--;
            locations.Dequeue();
        }
        //if locations is full then determine the variance and compare it to the threshold
        //determine if robot is stuck
        if (locations.Count == GetLocationsLimit())
        {
            float variance = GetVariance();
            Grapher.Log(variance, "variance", Color.white);
            if (variance < limit)
            {
                if (!CountIsStuck)
                {
                    CountIsStuck = true;
                }
                else
                {
                    Debug.LogError("Robot stuck");
                }
            }
        }
    }
}
