using Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    private bool trappedAlgorithmTriggered = false;
    void OnTriggerEnter(Collider collider)
    {
        if(!trappedAlgorithmTriggered && collider.tag == "Terrain")
        {
            //currently in sphere attached to head, find head, its associated ObjectConfig and we've got the whole robot
            trappedAlgorithmTriggered = true;
            ObjectConfig objConfig = this.transform.parent.gameObject.GetComponent<ObjectConfig>();
            RobotConfig robot = AIConfig.RobotConfigs.Where(r => r.RobotIndex == objConfig.RobotIndex).First();
            if (robot != null) robot.IsEnabled = true;
            Destroy(this);
        }
    }
}
