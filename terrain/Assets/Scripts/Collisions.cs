using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    private bool trappedAlgorithmTriggered = false;
    void OnTriggerEnter(Collider collider)
    {
        if(!trappedAlgorithmTriggered && collider.tag == "Terrain")
        {
            trappedAlgorithmTriggered = true;
            Transform head = transform.parent;
            RobotConfig robot = head.parent.gameObject.GetComponent<RobotConfig>();
            if (robot != null) robot.IsEnabled = true;
            Destroy(this);
        }
    }
}
