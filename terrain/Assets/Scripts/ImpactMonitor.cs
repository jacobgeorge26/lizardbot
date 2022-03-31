using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImpactMonitor : MonoBehaviour
{
    private int robotIndex;
    void Start()
    {
        ObjectConfig objConfig = gameObject.GetComponent<ObjectConfig>();
        try { robotIndex = AIConfig.RobotConfigs.Where(r => r.RobotIndex == objConfig.RobotIndex).First().RobotIndex; }
        catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
    }

    //https://stackoverflow.com/questions/36387753/getting-collision-contact-force
    void OnCollisionEnter(Collision col)
    {
        //determine if this force is just from the robot dropping in
        if(AIConfig.RobotConfigs[robotIndex].IsEnabled && Time.realtimeSinceStartup - Collisions.StartTimes[robotIndex] > 2)
        {
            //calculate force on body part
            Vector3 collisionForce = col.impulse / Time.fixedDeltaTime;
            float magnitude = collisionForce.magnitude;

            if (magnitude > 750)
            {
                AIConfig.RobotConfigs[robotIndex].PenaltyCount++;
                AIConfig.RobotConfigs[robotIndex].PenalisePerformance();
                Debug.LogWarning($"Adding a penalty to robot {AIConfig.RobotConfigs[robotIndex].Object.name}");
            }
        }
    }
}
