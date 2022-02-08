using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePopulation : MonoBehaviour
{
    private int HoldForFrames = 1000;
    private int delay = 0;
    private int RobotsGenerated = 0;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(RobotsGenerated < AIConfig.PopulationSize.Value)
        {
            delay--;
            if (delay == 0)
            {
                GenerateRobot();
            }
            delay = delay < 0 ? HoldForFrames : delay;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void GenerateRobot()
    {
        GameObject robot = new GameObject();
        int thisLayer = LayerMask.NameToLayer($"Robot{RobotsGenerated + 1}");
        robot.layer = thisLayer;
        robot.AddComponent<GenerateRobot>();
        RobotsGenerated++;
    }
}
