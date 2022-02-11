using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

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
        List<BaseVariable> genes = GetGenes(robot);

        if(AIConfig.RecombinationRate > 0) Recombination(genes);

        if(AIConfig.MutationRate > 0) Mutation(genes);

        //respawn
        robot.gameObject.SetActive(true);
    }
    private void Recombination(List<BaseVariable> genes)
    {
        
    }

    private void Mutation(List<BaseVariable> allGenes)
    {
        Random random = new Random();
        if(AIConfig.MutationType == global::Mutation.Any)
        {
            Array allMTypes = Enum.GetValues(typeof(Mutation));
            AIConfig.MutationType = (Mutation)allMTypes.GetValue(random.Next(allMTypes.Length - 1));
        }
        //trim list if physical / movement limited
        List<BaseVariable> genes = AIConfig.MutationType == global::Mutation.Physical ? allGenes.Where(g => g.Type == Variable.Physical).ToList() :
            AIConfig.MutationType == global::Mutation.Movement ? allGenes.Where(g => g.Type == Variable.Movement).ToList() :
                allGenes;
        foreach (BaseVariable gene in genes)
        {
            if(random.NextDouble() < AIConfig.MutationRate)
            {
                if(gene.GetType() == typeof(RangedVariable))
                {
                    //adjust
                    RangedVariable g = (RangedVariable)gene;
                    //handled in Variables - Vector3 will increment by a different amount for each axis
                    g.Increment();                    
                }
                else
                {
                    //toggle
                    gene.Value = !gene.Value;
                }
            }
        }
    }


    private List<BaseVariable> GetGenes(RobotConfig robot)
    {
        robot.IsTailEnabled.Value = false;
        List<BaseVariable> genes = new List<BaseVariable>();
        //split variables into physical and movement
        GetVariables(robot, genes);
        
        foreach (ObjectConfig objConfig in robot.Configs)
        {
            if(objConfig.Type == BodyPart.Body)
            {
                BodyConfig config = objConfig.Object.GetComponent<BodyConfig>();
                GetVariables(config, genes);
            }
            else if(objConfig.Type == BodyPart.Tail)
            {
                TailConfig config = objConfig.Object.GetComponent<TailConfig>();
                GetVariables(config, genes);
            }
        }
        //this is now a list of all variables that can be recombined / mutated
        //these can further be classified into those that affect the physical structure of the robot, versus the movement of it
        return genes;
    }

    private void GetVariables(dynamic config, List<BaseVariable> genes)
    {
        var allFields = config.GetType().GetFields();
        foreach (var item in allFields)
        {
            if(item.FieldType == typeof(BaseVariable)){
                var f = item.GetValue(config);
                genes.Add((BaseVariable)f);
            }
            else if (item.FieldType == typeof(RangedVariable)){
                var f = item.GetValue(config);
                genes.Add((RangedVariable)f);
            }
        }
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
