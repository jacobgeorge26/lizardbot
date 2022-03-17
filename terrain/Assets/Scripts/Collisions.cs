using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    bool started = false;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Terrain" && !started)
        {
            started = true;
            //currently in sphere attached to head, find head, its associated ObjectConfig and we've got the whole robot
            ObjectConfig objConfig = this.transform.parent.gameObject.GetComponent<ObjectConfig>();
            try
            {
                RobotConfig robot = AIConfig.RobotConfigs.Where(r => r.RobotIndex == objConfig.RobotIndex).First();
                if (robot != null) robot.IsEnabled = true;
            }
            catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }

        }
        else if(collider.tag == "Finish")
        {
            ObjectConfig objConfig = this.transform.parent.gameObject.GetComponent<ObjectConfig>();
            try
            {
                RobotConfig robot = AIConfig.RobotConfigs.First(r => r.RobotIndex == objConfig.RobotIndex);
                robot.RobotIsStuck();
            }
            catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
            Destroy(this);
        }
    }
}
