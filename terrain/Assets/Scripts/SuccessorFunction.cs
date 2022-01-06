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
    private Queue<float> variances = new Queue<float>();

    //these are used to determine how many frames are analysed
    //2 values are added to locations per sample
    //For a sample size of 50 and locationSize of 20 then 500 frames would be analysed
    private int sampleSize = 50;
    private int locationsSize = 100; 
    //this determines how far back the gradient is looking at. >2 is recommended as this leads to a lot of false positives
    private int variancesSize = 2;
    //these are used to prevent data being collected too soon 
    //the robot needs time to hit the terrain and react
    private bool IsEnabled = false;
    private int IsStuckBuffer = 300;

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
        IsStuckBuffer -= IsEnabled ? 1 : 0;
        if(IsStuckBuffer <= 0)
        {
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
    }

    private void UpdateLocations()
    {
        //get 3D pythagoras of how far from the origin the robot has travelled
        Vector3 currentLocation = GetComponent<Transform>().position;
        float magnitude2D = Mathf.Sqrt((currentLocation.x * currentLocation.x) + (currentLocation.z * currentLocation.z));
        float magnitude3D = Mathf.Sqrt(magnitude2D + (currentLocation.y * currentLocation.y));
        //add this to the current sample being collected
        tempLocations.Enqueue(magnitude3D);
        if (tempLocations.Count >= sampleSize)
        {
            //this sample is complete - store the max and min of this sample in locations
            locations.Enqueue(tempLocations.Max());
            locations.Enqueue(tempLocations.Min());
            tempLocations.Clear();

            //locations is full, bring back to size then check if robot is stuck
            if(locations.Count > locationsSize)
            {
                int count = 100;
                //only store as many samples in locations as determined in AI config
                while (locations.Count > locationsSize && count > 0)
                {
                    count--;
                    locations.Dequeue();
                }

                //get the variance of locations
                float variance = GetVariance();
                variances.Enqueue(variance);
                if (variances.Count > variancesSize)
                {
                    variances.Dequeue();
                }
                if (variances.Count == variancesSize)
                {
                    //get the gradient - the limit for what counts as low variance is variable between the terrains
                    //the gradient of the variance graph is not
                    float gradient = (variances.Last() - variances.First()) / variancesSize;
                    if (Math.Abs(gradient) < 0.00002)
                    {
                        GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
                        p.transform.position = currentLocation;
                        p.transform.parent = pointsContainer.transform;

                    }
                    else
                    {
                        GameObject p = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
                        p.transform.position = currentLocation;
                        p.transform.parent = pointsContainer.transform;
                    }
                }
            }

        }
    }
}
