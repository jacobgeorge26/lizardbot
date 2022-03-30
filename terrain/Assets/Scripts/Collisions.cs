using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    static bool[] started = new bool[AIConfig.PopulationSize];
    static bool[] finished = new bool[AIConfig.PopulationSize];
    internal static float[] StartTimes = new float[AIConfig.PopulationSize];

    private int robotIndex;

    void Start()
    {
        ObjectConfig objConfig = this.transform.parent.gameObject.GetComponent<ObjectConfig>();
        try { robotIndex = AIConfig.RobotConfigs.Where(r => r.RobotIndex == objConfig.RobotIndex).First().RobotIndex; }
        catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
        started[robotIndex] = false;
        finished[robotIndex] = false;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Terrain" && !started[robotIndex])
        {
            started[robotIndex] = true;
            RobotConfig robot = AIConfig.RobotConfigs[robotIndex];
            //currently in sphere attached to head, find head, its associated ObjectConfig and we've got the whole robot
            if (robot != null)
            {
                StartTimes[robot.RobotIndex] = Time.realtimeSinceStartup;
                robot.IsEnabled = true;
            }
        }
        else if(collider.tag == "Finish" && !finished[robotIndex])
        {
            finished[robotIndex] = true;
            RobotConfig robot = AIConfig.RobotConfigs[robotIndex];
            robot.SetPerformance();
            robot.RobotIsStuck(true);
            Destroy(this);
        }
    }

    public void Remove()
    {
        Destroy(this);
    }
}
