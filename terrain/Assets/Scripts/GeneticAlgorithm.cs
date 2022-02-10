using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
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
        robot.IsTailEnabled.Value = false;
        List<BaseVariable> toggle = new List<BaseVariable>();
        List<RangedVariable> adjust = new List<RangedVariable>();
        //split variables into physical and movement
        GetVariables(robot, toggle, adjust);
        
        foreach (ObjectConfig objConfig in robot.Configs)
        {
            if(objConfig.Type == BodyPart.Body)
            {
                BodyConfig config = objConfig.Object.GetComponent<BodyConfig>();
                GetVariables(config, toggle, adjust);
            }
            else if(objConfig.Type == BodyPart.Tail)
            {
                TailConfig config = objConfig.Object.GetComponent<TailConfig>();
                GetVariables(config, toggle, adjust);
            }
        }
        //there are now two lists of the toggleable and adjustable variables for the whole robot
        //these can further be classified into those that affect the physical structure of the robot, versus the movement of it
        Toggle(toggle.Where(f => f.Type == Variable.Movement).ToList());
        Adjust(adjust.Where(f => f.Type == Variable.Movement).ToList());
        if (AIConfig.MutatePhysical)
        {
            Toggle(toggle.Where(f => f.Type == Variable.Physical).ToList());
            Adjust(adjust.Where(f => f.Type == Variable.Physical).ToList());
        }
    }

    private void GetVariables(dynamic config, List<BaseVariable> toggle, List<RangedVariable> adjust)
    {
        var allFields = config.GetType().GetFields();
        foreach (var item in allFields)
        {
            if(item.FieldType == typeof(BaseVariable)){
                var f = item.GetValue(config);
                toggle.Add((BaseVariable)f);
            }
            else if (item.FieldType == typeof(RangedVariable)){
                var f = item.GetValue(config);
                adjust.Add((RangedVariable)f);
            }
        }
    }

    private void Toggle(List<BaseVariable> baseVariables)
    {
        throw new NotImplementedException();
    }

    private void Adjust(List<RangedVariable> rangedVariables)
    {
        throw new NotImplementedException();
    }
}
