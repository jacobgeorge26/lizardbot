using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RobotIsStuck(RobotConfig robot)
    {
        GenerateRobot generator = robot.gameObject.GetComponent<GenerateRobot>();
        GameObject prevObject = null;
        //pause objects
        foreach (Transform child in robot.gameObject.transform)
        {
            child.gameObject.SetActive(false);

            child.rotation = Quaternion.Euler(Vector3.zero);
            child.position = new Vector3(0, generator.GetYPos(), generator.GetZPos(prevObject, child.gameObject));

            Rigidbody childBody = child.gameObject.GetComponent<Rigidbody>();
            if(childBody != null)
            {
                childBody.velocity = Vector3.zero;
                childBody.angularVelocity = Vector3.zero;
                prevObject = child.gameObject;
            }

            //look through granchildren
            if(child.childCount > 0)
            {
                //look for trapped algorithm - needs to be reset
                TrappedAlgorithm trapped = child.gameObject.GetComponent<TrappedAlgorithm>();
                if(trapped != null)
                {
                    //remove and reattach so that start with retrigger
                    Destroy(trapped);
                    child.gameObject.AddComponent<TrappedAlgorithm>();
                }
                foreach (Transform grandchild in child.gameObject.transform)
                {
                    //look for head's sphere child - reattach Collisions script
                    SphereCollider collider = grandchild.gameObject.GetComponent<SphereCollider>();
                    if (collider != null && collider.isTrigger)
                    {
                        grandchild.gameObject.AddComponent<Collisions>();
                    }
                }
            }

        }

        //setup ready to respawn
        robot.IsEnabled = false;
        robot.Version++;
        robot.gameObject.name = $"robot{robot.RobotIndex + 1}_{robot.Version}";

        //mutate
        Mutate(robot);

        //respawn
        foreach (Transform child in robot.gameObject.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void Mutate(RobotConfig robot)
    {
    }
}
