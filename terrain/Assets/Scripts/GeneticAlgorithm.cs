using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GeneticAlgorithm : MonoBehaviour
{
    private RobotHelpers helpers;
    private GameObject[] LastRobots = new GameObject[AIConfig.PopulationSize];
    private UI ui;
    private CameraPosition cam;

    void Start()
    {
        ui = FindObjectOfType<UI>();
        cam = CameraConfig.RobotCamera.GetComponent<CameraPosition>();
    }
    public void RobotIsStuck(RobotConfig stuckRobot)
    {
        ui = FindObjectOfType<UI>();
        helpers = stuckRobot.gameObject.GetComponent<RobotHelpers>();
        //pause stuck robot
        Freeze(stuckRobot);

        int newVersion = stuckRobot.Version + 1;
        //init LastRobots if this is first iteration
        if (LastRobots[stuckRobot.RobotIndex] == null) LastRobots[stuckRobot.RobotIndex] = stuckRobot.gameObject;
        //clone better performing between the stuck (mutated) robot and its predecessor
        RobotConfig oldRobot = stuckRobot.MutationCount == AIConfig.MutationCycle ? CompareRobots(stuckRobot, ref newVersion) : stuckRobot;
        if(stuckRobot.RobotIndex == CameraConfig.CamFollow) CameraConfig.Hat.transform.parent = this.transform; //avoid the hat being cloned too
        GameObject newRobot = Instantiate(oldRobot.gameObject);
        RobotConfig robot = Init(newRobot, oldRobot, newVersion);
        helpers = newRobot.GetComponent<RobotHelpers>();
        helpers.Init(newRobot, robot);

        //mutate
        List<GeneVariable> genes = GetGenes(robot);

        if(AIConfig.RecombinationRate > 0) Recombination(genes);

        if(AIConfig.MutationRate > 0) Mutate(genes);

        UpdateBody(oldRobot.GetComponent<RobotConfig>(), robot);
        if(stuckRobot.MutationCount != AIConfig.MutationCycle && stuckRobot.Version != 0)
        {         
            if(oldRobot.Version > 0) Destroy(oldRobot.gameObject); //need access to info from original, leave disabled
            robot.MutationCount++;
        }

        //update UI for new
        ui.UpdateRobotUI(robot);

        //respawn
        robot.gameObject.SetActive(true);
    }

    private void Recombination(List<GeneVariable> genes)
    {
        
    }

    private void Mutate(List<GeneVariable> allGenes)
    {
        Random random = new Random();
        Mutation type = AIConfig.MutationType == Mutation.Any
            ? (Mutation)Enum.GetValues(typeof(Mutation)).GetValue(random.Next(Enum.GetValues(typeof(Mutation)).Length - 1))
            : AIConfig.MutationType;
        //trim list if physical / movement limited
        List<GeneVariable> genes = type == Mutation.Physical ? allGenes.Where(g => g.Type == Variable.Physical).ToList() :
            type == Mutation.Movement ? allGenes.Where(g => g.Type == Variable.Movement).ToList() :
                allGenes;
        foreach (GeneVariable gene in genes)
        {
            if(random.NextDouble() < AIConfig.MutationRate)
            {
                //adjust
                GeneVariable g = (GeneVariable)gene;
                //handled in Variables - Vector3 will increment by a different amount for each axis
                g.Increment();
            }
        }
    }


    private List<GeneVariable> GetGenes(RobotConfig robot)
    {
        robot.IsTailEnabled.Value = false;
        List<GeneVariable> genes = new List<GeneVariable>();
        //split variables into physical and movement
        GetVariables(robot, genes);
        
        foreach (ObjectConfig objConfig in robot.Configs)
        {
            if(objConfig.Type == BodyPart.Body)
            {
                BodyConfig config = objConfig.gameObject.GetComponent<BodyConfig>();
                GetVariables(config, genes);
            }
            else if(objConfig.Type == BodyPart.Tail)
            {
                TailConfig config = objConfig.gameObject.GetComponent<TailConfig>();
                GetVariables(config, genes);
            }
        }
        //this is now a list of all variables that can be recombined / mutated
        //these can further be classified into those that affect the physical structure of the robot, versus the movement of it
        return genes;
    }

    private void GetVariables(dynamic config, List<GeneVariable> genes)
    {
        var allFields = config.GetType().GetFields();
        foreach (var item in allFields)
        {
            if (item.FieldType == typeof(GeneVariable)){
                var f = item.GetValue(config);
                genes.Add((GeneVariable)f);
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
            child.position = new Vector3(0, helpers.GetYPos(), helpers.GetZPos(prevObject, child.gameObject));

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

    private RobotConfig CompareRobots(RobotConfig robot2, ref int v)
    {
        int index = robot2.RobotIndex;
        RobotConfig robot1 = LastRobots[index].GetComponent<RobotConfig>();
        v = Math.Max(robot1.Version, robot2.Version) + 1;
        //if the mutated robot performed as well or better than the previous one then continue with this one
        if(robot2.Performance >= robot1.Performance)
        {
            LastRobots[index] = robot2.gameObject;
            Destroy(robot1.gameObject);
            return robot2;
        }
        else
        {
            LastRobots[index] = robot1.gameObject;
            Destroy(robot2.gameObject);
            return robot1;
        }
    }

    private void UpdateBody(RobotConfig oldRobot, RobotConfig newRobot)
    {
        //are there more sections now?
        for (int i = 0; i < newRobot.NoSections.Value - oldRobot.NoSections.Value; i++)
        {
            helpers.CreateBody(oldRobot.NoSections.Value + i);
        }
        //are there fewer sections now?
        for (int i = 0; i < oldRobot.NoSections.Value - newRobot.NoSections.Value; i++)
        {
            helpers.RemoveBody(oldRobot.NoSections.Value - 1 - i);
        }
        //does the tail need to be added?
        if (!oldRobot.IsTailEnabled.Value && newRobot.IsTailEnabled.Value) helpers.CreateTail();
        //does the tail need to be removed?
        else if (oldRobot.IsTailEnabled.Value && !newRobot.IsTailEnabled.Value) helpers.RemoveTail();

        //update existing configs
        foreach (ObjectConfig item in newRobot.Configs)
        {
            if(item.Type == BodyPart.Body)
            {
                BodyConfig config = item.gameObject.GetComponent<BodyConfig>();
                helpers.UpdateBodyPart(config, item.Index, BodyPart.Body);
            }
            else if(item.Type == BodyPart.Tail)
            {
                TailConfig config = item.gameObject.GetComponent<TailConfig>();
                helpers.UpdateBodyPart(config, 0, BodyPart.Tail);
            }
        }
        //if the robot camera is following this robot then update its Head & Tail variables
        if (CameraConfig.CamFollow == newRobot.RobotIndex) cam.SetRobot(newRobot);
    }

    private RobotConfig Init(GameObject newRobot, RobotConfig oldRobot, int newVersion)
    {
        RobotConfig robot = newRobot.GetComponent<RobotConfig>();
        //setup ready to respawn
        robot.IsEnabled = false;
        robot.Version = newVersion;
        robot.gameObject.name = $"Robot {robot.RobotIndex + 1} V {robot.Version}";
        robot.transform.parent = oldRobot.transform.parent;
        robot.Performance = 0;
        robot.MutationCount = 0;
        robot.Original = oldRobot.Original;

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
