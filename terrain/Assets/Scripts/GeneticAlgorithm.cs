using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public void RobotIsStuck(RobotConfig stuckRobot)
    {
        //pause stuck robot
        Freeze(stuckRobot);

        //clone better performing between the stuck (mutated) robot and its predecessor
        GameObject oldRobot = CompareRobots(stuckRobot);
        GameObject newRobot = Instantiate(oldRobot);
        RobotConfig robot = Init(newRobot, oldRobot);

        //mutate
        Mutate(robot);

        //respawn
        robot.gameObject.SetActive(true);
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
       
    }

    private void Adjust(List<RangedVariable> rangedVariables)
    {

    }

    private void Freeze(RobotConfig robot)
    {
        robot.gameObject.SetActive(false);
        GameObject prevObject = null;
        //pause objects       
        foreach (Transform child in robot.gameObject.transform)
        {
            child.rotation = Quaternion.Euler(Vector3.zero);
            child.position = new Vector3(0, robot.GetYPos(), robot.GetZPos(prevObject, child.gameObject));

            Rigidbody childBody = child.gameObject.GetComponent<Rigidbody>();
            if (childBody != null)
            {
                childBody.velocity = Vector3.zero;
                childBody.angularVelocity = Vector3.zero;
                prevObject = child.gameObject;
            }

            //look through granchildren
            if (child.childCount > 0)
            {
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
    }

    private GameObject CompareRobots(RobotConfig robot1)
    {
        if (robot1.LastRobot == null) return robot1.gameObject;
        RobotConfig robot2 = robot1.LastRobot.GetComponent<RobotConfig>();
        //if the mutated robot performed as well or better than the previous one then continue with this one
        if(robot2.Performance > robot1.Performance)
        {
            //up the version number to allow the clone to +1
            robot2.GetComponent<RobotConfig>().Version = robot1.Version;
            Destroy(robot1.gameObject);
            return robot2.gameObject;
        }
        else
        {
            Destroy(robot2.gameObject);
            return robot1.gameObject;
        }
    }

    private RobotConfig Init(GameObject newRobot, GameObject oldRobot)
    {
        RobotConfig robot = newRobot.GetComponent<RobotConfig>();
        //setup ready to respawn
        robot.IsEnabled = false;
        robot.LastRobot = oldRobot;
        robot.Version++;
        robot.gameObject.name = $"Robot {robot.RobotIndex + 1} V {robot.Version}";
        robot.transform.parent = oldRobot.transform.parent;
        robot.Performance = 0;

        //replace stuck robot with new robot in RobotConfigs
        int index = AIConfig.RobotConfigs.IndexOf(AIConfig.RobotConfigs.First(c => c.RobotIndex.Equals(robot.RobotIndex)));
        AIConfig.RobotConfigs[index] = robot;

        //fill RobotConfig.Configs with the ObjectConfigs of each body part
        foreach (Transform item in newRobot.transform)
        {
            ObjectConfig objConfig = item.gameObject.GetComponent<ObjectConfig>();
            if (objConfig != null) robot.Configs.Add(objConfig);
        }
        return robot;
    }
}
