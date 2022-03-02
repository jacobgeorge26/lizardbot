using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GeneticAlgorithm : object
{
    private static UI ui;

    public static void RobotIsStuck(this RobotConfig stuckRobot)
    {
        ui ??= UIConfig.UIContainer.GetComponent<UI>();
        //pause stuck robot
        Freeze(stuckRobot);

        int newVersion = stuckRobot.Version + 1;
        //init LastRobots if this is first iteration
        if (AIConfig.LastRobots[stuckRobot.RobotIndex] == null) AIConfig.LastRobots[stuckRobot.RobotIndex] = stuckRobot;
        //clone better performing between the stuck (mutated) robot and its predecessor
        RobotConfig oldRobot = stuckRobot.MutationCount == AIConfig.MutationCycle ? CompareRobots(stuckRobot, ref newVersion) : stuckRobot;

        if (stuckRobot.RobotIndex == CameraConfig.CamFollow) CameraConfig.Hat.transform.parent = CameraConfig.RobotCamera.transform; //avoid the hat being cloned too
        ObjectConfig firstObjConfig = oldRobot.Configs.First();
        GameObject newRobotObj = firstObjConfig.Clone(oldRobot.Object);
        RobotConfig newRobot = new RobotConfig(oldRobot.RobotIndex, newRobotObj);
        newRobot = Init(newRobot, oldRobot, newVersion);

        //mutate
        List<Gene> genes = new List<Gene>();
        if (AIConfig.RecombinationRate > 0 && AIConfig.PopulationSize > 1) genes = Recombine(newRobot, oldRobot);
        else
        {
            genes = GetGenes(newRobot, true);
        }
        if (AIConfig.MutationRate > 0) Mutate(genes);

        UpdateBody(oldRobot, newRobot);
        if (stuckRobot.MutationCount != AIConfig.MutationCycle && stuckRobot.Version != 0)
        {
            //need access to info from original, leave disabled
            //otherwise destroy
            if (oldRobot.Version > 0) oldRobot.Configs.First().Remove(oldRobot.Object);
            newRobot.MutationCount++;
        }

        //update UI for new
        ui.UpdateRobotUI(newRobot);

        //respawn
        newRobot.Object.SetActive(true);
    }

    private static List<Gene> Recombine(RobotConfig robot, RobotConfig old)
    {
        Recombination type = AIConfig.RecombinationType == Recombination.Any
            ? (Recombination)Enum.GetValues(typeof(Recombination)).GetValue((int)Random.Range(0, Enum.GetValues(typeof(Recombination)).Length - 1))
            : AIConfig.RecombinationType;
        RobotConfig best1 = type switch
        {
            Recombination.PhysicalLikeness => BestPhysicalRobot(robot),
            Recombination.MovementLikeness => BestMovementRobot(robot),
            Recombination.Triad => BestPhysicalRobot(robot),
            Recombination.Lizard => BestLizardRobot(robot),
            _ => AIConfig.RobotConfigs.Where(r => r.RobotIndex != robot.RobotIndex).ToList()[(int)Random.Range(0, AIConfig.PopulationSize - 2)]
        };
        //if using triad approach then best1 is the physical one, best2 needs to be the movement one
        RobotConfig best2 = type == Recombination.Triad ? BestMovementRobot(robot) : null;
        //Debug.Log($"Robot: {robot.RobotIndex}    Physical: {(best1 == null ? '-' : best1.RobotIndex)}    Movement: {(best2 == null ? '-' : best2.RobotIndex)}");

        //recombine robot
        List<Gene> newRobotGenes = new List<Gene>();
        //body
        for (int i = 0; i < robot.NoSections.Value; i++)
        {
            //because the nosections might have just been increased it cannot be assumed that a config for this section exists yet
            ObjectConfig body = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == i);
            ObjectConfig body1 = best1 != null && best1.NoSections.Value > i ? best1.Configs.First(o => o.Type == BodyPart.Body && o.Index == i) : null;
            ObjectConfig body2 = best2 != null && best2.NoSections.Value > i ? best2.Configs.First(o => o.Type == BodyPart.Body && o.Index == i) : null;
            CombineGenes(GetGenes(robot, body), GetGenes(best1, body1), GetGenes(best2, body2), ref newRobotGenes);
        }
        //tail
        if (robot.IsTailEnabled.Value)
        {
            ObjectConfig tail = robot.Configs.First(o => o.Type == BodyPart.Tail);
            ObjectConfig tail1 = best1 != null && best1.IsTailEnabled.Value ? best1.Configs.First(o => o.Type == BodyPart.Tail) : null;
            ObjectConfig tail2 = best2 != null && best2.IsTailEnabled.Value ? best2.Configs.First(o => o.Type == BodyPart.Tail) : null;
            CombineGenes(GetGenes(robot, tail), GetGenes(best1, tail1), GetGenes(best2, tail2), ref newRobotGenes);
        }
        //robot
        CombineGenes(GetGenes(robot, false), GetGenes(best1, false), GetGenes(best2, false), ref newRobotGenes);
        return newRobotGenes;
    }

    private static void CombineGenes(List<Gene> genes1, List<Gene> genes2, List<Gene> genes3, ref List<Gene> newGenes)
    {
        //genes1 is original robot, use this for loop to make sure only genes that the robot has are used. 
        //this shouldn't make a difference, but avoids strange errors
        for (int i = 0; i < genes1.Count; i++)
        {
            Gene oldGene = genes1[i];
            if (Random.value < AIConfig.RecombinationRate)
            {
                //recombine with another gene
                //50/50 between genes2 or genes3, defaulting to the existing gene if they're empty
                Gene newGene;
                if(genes2.Count > 0 && genes3.Count > 0)
                {
                    newGene = Random.value < 0.5f ? genes2.First(g => g.Type == oldGene.Type) : genes3.First(g => g.Type == oldGene.Type);
                }
                else
                {
                    newGene = genes2.Count > 0 ? genes2.First(g => g.Type == oldGene.Type)
                        : genes3.Count > 0 ? genes3.First(g => g.Type == oldGene.Type)
                            : oldGene;
                }
                oldGene.Value = newGene.Real;
                newGenes.Add(newGene);
            }
            else
            {
                //maintain original
                newGenes.Add(oldGene);
            }
        }
    }

    private static void Mutate(List<Gene> allGenes)
    {
        Mutation type = AIConfig.MutationType == Mutation.Any
            ? (Mutation)Enum.GetValues(typeof(Mutation)).GetValue((int)Random.Range(0, Enum.GetValues(typeof(Mutation)).Length - 1))
            : AIConfig.MutationType;
        //trim list if physical / movement limited
        List<Gene> genes = type == Mutation.Physical ? allGenes.Where(g => g.Type < 0).ToList() :
            type == Mutation.Movement ? allGenes.Where(g => g.Type > 0).ToList() :
                allGenes;
        foreach (Gene gene in genes)
        {
            if(Random.value < AIConfig.MutationRate)
            {
                //adjust
                Gene g = (Gene)gene;
                //handled in Variables - Vector3 will increment by a different amount for each axis
                g.Increment();
            }
        }
    }


    private static List<Gene> GetGenes(RobotConfig robot, bool getObjGenes)
    {
        List<Gene> genes = new List<Gene>();
        //split variables into physical and movement
        genes = genes.Concat(robot.GetVariables(robot)).ToList();
        //if we want the full list of genes then get the genes for the object configs too
        if (getObjGenes)
        {
            robot.Configs.ForEach(o => genes = genes.Concat(GetGenes(robot, o)).ToList());
        }
        //this is now a list of all variables that can be recombined / mutated
        //these can further be classified into those that affect the physical structure of the robot, versus the movement of it
        return genes;
    }

    private static List<Gene> GetGenes(RobotConfig robot, ObjectConfig config)
    {
        if (config == null) return new List<Gene>();
        return config.Type switch
        {
            BodyPart.Body => robot.GetVariables(config.Body),
            BodyPart.Tail => robot.GetVariables(config.Tail),
            _ => null
        };
    }

    private static RobotConfig BestLizardRobot(RobotConfig robot)
    {
        int attempts = 1;
        List<GameObject> nearby = new List<GameObject>();
        List<RobotConfig> robots = new List<RobotConfig>();
        //look for robots nearby until selection size is met
        while (nearby.Count < AIConfig.SelectionSize && attempts <= 5)
        {
            //get robots in that vicinity
            nearby = robot.GetNearbyRobots(attempts * 10);
            attempts++;
        }
        //nearby currently contains the heads, I need the robot to get the RobotConfig
        nearby.ForEach(r => robots.Add(r.transform.parent.gameObject.GetComponent<RobotConfig>()));
        //return robot with best body colour
        robots.OrderBy(r => r.BodyColour);
        return robots.Count > 0 ? robots.First() : null;
    }

    private static RobotConfig BestMovementRobot(RobotConfig robot)
    {
        int attempts = 0;
        List<RobotConfig> robots = new List<RobotConfig>();
        while (robots.Count < AIConfig.SelectionSize && attempts < 5)
        {
            robots = robot.GetMovementSimilarRobots(attempts);
            attempts++;
        }
        //return robot with best performance
        robots.OrderBy(r => r.Performance);
        return robots.Count > 0 ? robots.Last() : null;
    }

    private static RobotConfig BestPhysicalRobot(RobotConfig robot)
    {
        int attempts = 0;
        List<RobotConfig> robots = new List<RobotConfig>();
        //look for robots nearby until selection size is met
        while (robots.Count < AIConfig.SelectionSize && attempts < 5)
        {
            //get robots that are similar
            robots = robot.GetPhysicallySimilarRobots(attempts);
            //out of those robots, 
            attempts++;
        }
        //return robot with best performance
        robots.OrderBy(r => r.Performance);
        return robots.Count > 0 ? robots.Last() : null;
    }

    private static void Freeze(RobotConfig robot)
    {
        robot.Object.SetActive(false);
        //pause objects       
        foreach (ObjectConfig childConfig in robot.Configs.OrderBy(o => o.Type))
        {
            Transform child = childConfig.gameObject.transform;
            GameObject prevObject = null;
            if(!(childConfig.Type == BodyPart.Body && childConfig.Index == 0))
            {
                int index = childConfig.Type == BodyPart.Body ? childConfig.Index - 1 : robot.NoSections.Value - 1;
                prevObject = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == index).gameObject;
            }
            child.rotation = Quaternion.Euler(Vector3.zero);
            child.position = new Vector3(0, robot.GetYPos(), robot.GetZPos(prevObject, child.gameObject));

            Rigidbody childBody = child.gameObject.GetComponent<Rigidbody>();
            if (childBody != null)
            {
                childBody.velocity = Vector3.zero;
                childBody.angularVelocity = Vector3.zero;
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

    private static RobotConfig CompareRobots(RobotConfig robot2, ref int v)
    {
        int index = robot2.RobotIndex;
        RobotConfig robot1 = AIConfig.LastRobots[index];
        v = Math.Max(robot1.Version, robot2.Version) + 1;
        //if the mutated robot performed as well or better than the previous one then continue with this one
        if(robot2.Performance >= robot1.Performance)
        {
            AIConfig.LastRobots[index] = robot2;
            robot1.Configs.First().Remove(robot1.Object);
            return robot2;
        }
        else
        {
            AIConfig.LastRobots[index] = robot1;
            robot2.Configs.First().Remove(robot2.Object);
            return robot1;
        }
    }

    private static void UpdateBody(RobotConfig oldRobot, RobotConfig newRobot)
    {
        //are there more sections now?
        for (int i = 0; i < newRobot.NoSections.Value - oldRobot.NoSections.Value; i++)
        {
            int index = oldRobot.NoSections.Value + i;
            newRobot.CreateBody(index);
            newRobot.AverageRestOfBody();
        }
        //are there fewer sections now?
        for (int i = 0; i < oldRobot.NoSections.Value - newRobot.NoSections.Value; i++)
        {
            int index = oldRobot.NoSections.Value - 1 - i;
            newRobot.RemoveBody(index);
        }
        //does the tail need to be added?
        if (!oldRobot.IsTailEnabled.Value && newRobot.IsTailEnabled.Value) newRobot.CreateTail();
        //does the tail need to be removed?
        else if (oldRobot.IsTailEnabled.Value && !newRobot.IsTailEnabled.Value) newRobot.RemoveTail();

        //update existing configs
        foreach (ObjectConfig item in newRobot.Configs)
        {
            if(item.Type == BodyPart.Body)
            {
                newRobot.UpdateBodyPart(item, item.Index, BodyPart.Body);
            }
            else if(item.Type == BodyPart.Tail)
            {
                newRobot.UpdateBodyPart(item, 0, BodyPart.Tail);
            }
        }
        if (newRobot.MaintainSerpentine.Value) newRobot.MakeSerpentine(false);
        //if the robot camera is following this robot then update its Head & Tail variables
        if (CameraConfig.CamFollow == newRobot.RobotIndex && CameraConfig.RobotCamera.activeSelf == true) CameraConfig.RobotCamera.GetComponent<CameraPosition>().SetRobot(newRobot);
    }

    private static RobotConfig Init(RobotConfig newRobot, RobotConfig oldRobot, int newVersion)
    {
        newRobot.FreshCopy(oldRobot, newVersion);
        //setup ready to respawn
        newRobot.Object.name = $"Robot {newRobot.RobotIndex + 1} V {newRobot.Version}";
        newRobot.Object.transform.parent = oldRobot.Object.transform.parent;

        //replace stuck robot with new robot in RobotConfigs
        int index = AIConfig.RobotConfigs.IndexOf(AIConfig.RobotConfigs.First(c => c.RobotIndex.Equals(newRobot.RobotIndex)));
        AIConfig.RobotConfigs[index] = newRobot;

        //fill RobotConfig.Configs with the ObjectConfigs of each body part
        foreach (Transform item in newRobot.Object.transform)
        {
            ObjectConfig objConfig = item.gameObject.GetComponent<ObjectConfig>();
            if (objConfig != null)
            {
                ObjectConfig oldObjConfig = oldRobot.Configs.First(o => o.Type == objConfig.Type && o.Index == objConfig.Index);
                if (objConfig.Type == BodyPart.Body)
                {
                    objConfig.Body = new BodyConfig();
                    objConfig.Body.Clone(oldObjConfig.Body);
                }
                else if (objConfig.Type == BodyPart.Tail)
                {
                    objConfig.Tail = new TailConfig();
                    objConfig.Tail.Clone(oldObjConfig.Tail);
                }
                newRobot.Configs.Add(objConfig);
            }
        }
        return newRobot;
    }
}
