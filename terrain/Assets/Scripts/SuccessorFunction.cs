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
    private float limit;

    void Start()
    {
        //the limit is the float that, if max - min of locations falls under will determine that the robot is stuck
        //it uses the noise frequency of the terrain - which is calculated using the terrain type and terrain size
        //the larger the noise the rougher the terrain
        //this is multiplied here and then inversed - so the limit will be smaller on rougher terrain
        //when 'craters' are created in the terrain this equates to 'is the robot stuck within a quarter of this crater?'
        limit = 1 / (TerrainConfig.GetNoiseFrequency() * 4);
    }

    void Update()
    {
        frameRate = ((int)(1.0f / Time.deltaTime));
        UpdateLocations();
        if (locations.Count == GetLocationsLimit())
        {
            DetermineRobotStuck();
        }
    }

    private void DetermineRobotStuck()
    {
        //if the average from the last x frames is under the limit then the robot is bouncing and is stuck
        if(locations.Max() - locations.Min() < limit)
        {
            //TODO: when the AI is setup make this respawn
            Debug.LogError("Robot Stuck");
        }
    }

    private int GetLocationsLimit()
    {
        //the number of samples collected is based on the frame rate - to roughly equate to the number of seconds. 
        //e.g. we want the last ~second to be analysed and the framerate is 300. 1 sample = 50 frames. 
        //2 values are added to locations each sample
        //the max size of locations should be 12
        //the minimum value will always be 10 to accomodate lower frame ratee
        return Math.Max((int)(frameRate / 50 * 2), 10);
    }

    private void UpdateLocations()
    {
        //get 3D pythagoras of how far from the origin the robot has travelled
        Vector3 currentLocation = GetComponent<Transform>().position;
        float magnitude2D = Mathf.Sqrt((currentLocation.x * currentLocation.x) + (currentLocation.z * currentLocation.z));
        float magnitude3D = Mathf.Sqrt(magnitude2D + (currentLocation.y * currentLocation.y));
        //add this to the current sample being collected
        tempLocations.Enqueue(magnitude3D);
        if(tempLocations.Count >= 50)
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
    }
}
