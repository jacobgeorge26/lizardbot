using Config;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GeneticAlgorithm : object
{
    private static UIDisplay ui;

    public static void RobotIsStuck(this RobotConfig stuckRobot, bool respawnOnly = false)
    {
        stuckRobot.IsEnabled = false;

        ui ??= UIConfig.UIContainer.GetComponent<UIDisplay>();
        //pause stuck robot
        stuckRobot.Object.SetActive(false);

        RobotConfig newRobot;
        if (!respawnOnly) newRobot = PerformGA(stuckRobot);
        else newRobot = stuckRobot;

        //////////////
        newRobot.MaintainSerpentine.Value = true;

        //respawn
        Reset(newRobot);
        newRobot.Object.SetActive(true);
    }

    private static RobotConfig PerformGA(RobotConfig stuckRobot)
    {
        //update BestRobot
        if (DebugConfig.LogRobotData && (DebugConfig.BestRobot == null || stuckRobot.Performance > DebugConfig.BestRobot.Performance))
        {
            RobotConfig prevBest = DebugConfig.BestRobot;
            DebugConfig.BestRobot = stuckRobot;

            //delete other one
            if(prevBest != null && prevBest.Object != null && prevBest.Version > 0)
            {
                try { prevBest.Configs.First().Remove(prevBest.Object); }
                catch (Exception) { Debug.LogWarning($"Check that Robot{prevBest.RobotIndex + 1}V{prevBest.Version} has been deleted correctly. It has been replaced as the best robot."); }
            }
        }

        int newVersion = stuckRobot.Version + 1;
        //init LastRobots if this is first iteration
        if (AIConfig.LastRobots[stuckRobot.RobotIndex] == null) AIConfig.LastRobots[stuckRobot.RobotIndex] = stuckRobot;
        //clone better performing between the stuck (mutated) robot and its predecessor
        RobotConfig oldRobot = stuckRobot.MutationCount == AIConfig.MutationCycle ? CompareRobots(stuckRobot, ref newVersion) : stuckRobot;

        if (stuckRobot.RobotIndex == CameraConfig.CamFollow) CameraConfig.Hat.transform.parent = CameraConfig.RobotCamera.transform; //avoid the hat being cloned too

        ObjectConfig firstObjConfig = null;
        GameObject newRobotObj;
        try { 
            firstObjConfig = oldRobot.Configs.First();
            newRobotObj = firstObjConfig.Clone(oldRobot.Object);
        }
        catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), stuckRobot); return stuckRobot; }     
        RobotConfig newRobot = new RobotConfig(oldRobot.RobotIndex, newRobotObj);
        newRobot = Init(newRobot, oldRobot, newVersion);

        //mutate
        Mutation type = AIConfig.MutationType == Mutation.Any
            ? (Mutation)Enum.GetValues(typeof(Mutation)).GetValue((int)Random.Range(0, Enum.GetValues(typeof(Mutation)).Length - 1))
            : AIConfig.MutationType;
        List<Gene> genes = new List<Gene>();
        if (AIConfig.RecombinationRate > 0 && AIConfig.PopulationSize > 1) genes = Recombine(newRobot, oldRobot, type);
        else
        {
            genes = GetGenes(newRobot, true);
        }
        if (AIConfig.MutationRate > 0) Mutate(genes, type);

        UpdateBody(oldRobot, newRobot);
        if (stuckRobot.MutationCount != AIConfig.MutationCycle && stuckRobot.Version != 0)
        {
            //need access to info from original, leave disabled
            //otherwise destroy
            try { 
                if (oldRobot.Version > 0 && DebugConfig.BestRobot != oldRobot)
                {
                    oldRobot.Configs.First().Remove(oldRobot.Object);
                }
            }
            catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return stuckRobot; }

            newRobot.MutationCount++;
        }

        //update UI for new
        ui.UpdateRobotUI(newRobot);

        return newRobot;
    }

    private static List<Gene> Recombine(RobotConfig robot, RobotConfig old, Mutation mutationType)
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
            ObjectConfig body = null, body1 = null, body2 = null;
            try { body = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == i); }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return new List<Gene>(); }
            try { body1 = best1 != null && best1.NoSections.Value > i ? best1.Configs.First(o => o.Type == BodyPart.Body && o.Index == i) : null; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), best1); return new List<Gene>(); }
            try { body2 = best2 != null && best2.NoSections.Value > i ? best2.Configs.First(o => o.Type == BodyPart.Body && o.Index == i) : null; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), best2); return new List<Gene>(); }
            List<Gene> bodyGenes = FilterGenes(GetGenes(robot, body), mutationType);
            List<Gene> body1Genes = FilterGenes(GetGenes(best1, body1), mutationType);
            List<Gene> body2Genes = FilterGenes(GetGenes(best2, body2), mutationType);
            CombineGenes(bodyGenes, body1Genes, body2Genes, ref newRobotGenes);
        }
        //tail
        //for some reason very occasionally robots lose their tails
        //it's stumping me - and only crops up here
        //I've chosen to respawn the robot and skip the recombination
        //not ideal, I know
        if (robot.IsTailEnabled.Value)
        {
            ObjectConfig tail = null, tail1 = null, tail2 = null;
            try { tail = robot.Configs.First(o => o.Type == BodyPart.Tail); }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return new List<Gene>(); }
            try { tail1 = best1 != null && best1.IsTailEnabled.Value ? best1.Configs.First(o => o.Type == BodyPart.Tail) : null; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), best1); return new List<Gene>(); }
            try { tail2 = best2 != null && best2.IsTailEnabled.Value ? best2.Configs.First(o => o.Type == BodyPart.Tail) : null; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), best2); return new List<Gene>(); }
            List<Gene> tailGenes = FilterGenes(GetGenes(robot, tail), mutationType);
            List<Gene> tail1Genes = FilterGenes(GetGenes(best1, tail1), mutationType);
            List<Gene> tail2Genes = FilterGenes(GetGenes(best2, tail2), mutationType);
            CombineGenes(tailGenes, tail1Genes, tail2Genes, ref newRobotGenes);
        }
        //robot
        List<Gene> robotGenes = FilterGenes(GetGenes(robot, false), mutationType);
        List<Gene> best1Genes = FilterGenes(GetGenes(best1, false), mutationType);
        List<Gene> best2Genes = FilterGenes(GetGenes(best2, false), mutationType);
        CombineGenes(robotGenes, best1Genes, best2Genes, ref newRobotGenes);
        if (type == Recombination.Lizard && mutationType != Mutation.Movement) CombineBodyColour(robot, best1, best2, ref newRobotGenes);
        return newRobotGenes;
    }

    private static List<Gene> FilterGenes(List<Gene> genes, Mutation mutationType)
    {
        //trim list if physical / movement limited
        genes = mutationType == Mutation.Physical ? genes.Where(g => g.Type < 0).ToList() :
            mutationType == Mutation.Movement ? genes.Where(g => g.Type > 0).ToList() :
                genes;
        return genes;
    }

    private static void CombineBodyColour(RobotConfig robot, RobotConfig best1, RobotConfig best2, ref List<Gene> newRobotGenes)
    {
        Gene colourGene;
        try { colourGene = newRobotGenes.First(g => g.Type == Variable.BodyColour); }
        catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
        //get min and max of recombination
        float min = 255, max = 0;
        min = robot.BodyColour.Value < min ? robot.BodyColour.Value : min;
        min = best1 != null && best1.BodyColour.Value < min ? best1.BodyColour.Value : min;
        min = best2 != null && best2.BodyColour.Value < min ? best2.BodyColour.Value : min;
        max = robot.BodyColour.Value > max ? robot.BodyColour.Value : max;
        max = best1 != null && best1.BodyColour.Value > max ? best1.BodyColour.Value : max;
        max = best2 != null && best2.BodyColour.Value > max ? best2.BodyColour.Value : max;
        //add 5% of range either side to account for variation
        max = max + ((robot.BodyColour.Max - robot.BodyColour.Min) * 0.05f);
        min = min - ((robot.BodyColour.Max - robot.BodyColour.Min) * 0.05f);
        //set the body colour as a random value within this range
        colourGene.Value = Random.Range(min, max);
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
                Gene newGene = null;
                if(genes2.Count > 0 && genes3.Count > 0)
                {
                    try { newGene = Random.value < 0.5f ? genes2.First(g => g.Type == oldGene.Type) : genes3.First(g => g.Type == oldGene.Type); }
                    catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
                }
                else
                {
                    try
                    {
                        newGene = genes2.Count > 0 ? genes2.First(g => g.Type == oldGene.Type)
                        : genes3.Count > 0 ? genes3.First(g => g.Type == oldGene.Type)
                            : oldGene;
                    }
                    catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
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

    private static void Mutate(List<Gene> allGenes, Mutation type)
    {
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
            BodyPart.Leg => robot.GetVariables(config.Leg),
            _ => new List<Gene>()
        };
    }

    private static RobotConfig BestLizardRobot(RobotConfig robot)
    {
        int attempts = 1;
        List<RobotConfig> robots = new List<RobotConfig>();
        //look for robots nearby until selection size is met
        while (robots.Count < AIConfig.SelectionSize && attempts <= 5)
        {
            //get robots in that vicinity
            robots = robot.GetNearbyRobots(attempts * 10);
            attempts++;
        }
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
            if (robots == null) {
                return null; //an exception has occurred and the robot is being respawned
            }
            //out of those robots, 
            attempts++;
        }
        //return robot with best performance
        robots.OrderBy(r => r.Performance);
        return robots.Count > 0 ? robots.Last() : null;
    }

    private static void Reset(RobotConfig robot)
    {
        Vector3 spawnPoint = TerrainConfig.GetSpawnPoint(robot.RobotIndex);
        //pause objects       
        foreach (ObjectConfig childConfig in robot.Configs.OrderBy(o => o.Type))
        {
            Transform child = childConfig.gameObject.transform;
            ObjectConfig prevObj = null;
            GameObject prevBody = null;
            if (childConfig.Type == BodyPart.Leg)
            {
                try { 
                    prevObj = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == childConfig.Leg.AttachedBody);
                    prevBody = prevObj.gameObject;
                }
                catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return; }
                robot.SetLegPosition(childConfig.gameObject, childConfig.Leg, prevBody, prevObj.Body.LegPoints[(int)childConfig.Leg.Position]);
            }
            else
            {
                //for tail and body parts past head, get the prevsection
                if(!(childConfig.Type == BodyPart.Body && childConfig.Index == 0))
                {
                    int index = childConfig.Type == BodyPart.Body ? childConfig.Index - 1 : robot.NoSections.Value - 1;
                    try { 
                        prevObj = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == index);
                        prevBody = prevObj.gameObject;
                    }
                    catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return; }
                }
                child.rotation = Quaternion.Euler(Vector3.zero);
                child.position = new Vector3(spawnPoint.x, robot.GetYPos(), spawnPoint.z + robot.GetZPos(prevBody, child.gameObject));
            }

            //set zero velocity
            Rigidbody childBody = child.gameObject.GetComponent<Rigidbody>();
            if (childBody != null)
            {
                childBody.velocity = Vector3.zero;
                childBody.angularVelocity = Vector3.zero;
            }

            //look through grandchildren
            if (child.childCount > 0)
            {
                foreach (Transform grandchild in child.gameObject.transform)
                {
                    //look for head's sphere child - reattach Collisions script
                    SphereCollider collider = grandchild.gameObject.GetComponent<SphereCollider>();
                    if (collider != null && collider.isTrigger)
                    {
                        //need to delete existing one first
                        Collisions collisions = grandchild.gameObject.GetComponent<Collisions>();
                        if (collisions != null) collisions.Remove();
                        grandchild.gameObject.AddComponent<Collisions>();
                    }
                }
            }
        }
        robot.SetChildLayer(robot.Object.layer);
        robot.StartTime = Time.time;
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
            try { robot1.Configs.First().Remove(robot1.Object); }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot1); }
            
            return robot2;
        }
        else
        {
            AIConfig.LastRobots[index] = robot1;
            try { robot2.Configs.First().Remove(robot2.Object); }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot2); }     
            return robot1;
        }
    }

    private static void UpdateBody(RobotConfig oldRobot, RobotConfig newRobot)
    {
        //are there more sections now?
        for (int i = 0; i < newRobot.NoSections.Value - oldRobot.NoSections.Value; i++)
        {
            int index = oldRobot.NoSections.Value + i;
            BodyConfig newBody = newRobot.CreateBody(index);
            newRobot.AverageRestOfBody(newBody);
        }
        //are there fewer sections now?
        for (int i = 0; i < oldRobot.NoSections.Value - newRobot.NoSections.Value; i++)
        {
            int index = oldRobot.NoSections.Value - 1 - i;
            newRobot.RemoveBody(index);
        }
        //does the tail need to be added?
        if(newRobot.IsTailEnabled.Value && newRobot.Configs.Where(o => o.Type == BodyPart.Tail).ToList().Count == 0) newRobot.CreateTail();
        //does the tail need to be removed?
        else if (oldRobot.IsTailEnabled.Value && !newRobot.IsTailEnabled.Value) newRobot.RemoveTail();

        newRobot.ValidateLegParams();
        //are there more legs now?
        for (int i = 0; i < newRobot.NoLegs.Value - oldRobot.NoLegs.Value; i++)
        {
            int index = oldRobot.NoLegs.Value + i;
            int NoSpawnPoints = 2;
            List<int> legIndexes = newRobot.UniformBody.Value ? Enumerable.Range(1, (int)newRobot.NoSections.Value - 1).ToList() : Enumerable.Range(2, (int)(newRobot.NoSections.Value - 1) * 2).ToList();
            foreach (var leg in newRobot.Configs.Where(o => o.Type == BodyPart.Leg))
            {
                if (newRobot.UniformBody.Value) legIndexes.Remove(leg.Leg.AttachedBody);
                else
                {
                    if (leg.Leg.Position == LegPosition.Left) legIndexes.Remove(leg.Leg.AttachedBody * 2);
                    else legIndexes.Remove((leg.Leg.AttachedBody * 2) + 1);
                }
            }
            if (legIndexes.Count == 0)
            {
                //weird bug here where sometimes the no legs is 1 when it should be zero
                //haven't had time to track it down - doesn't seem to be an issue with ValidateParams
                newRobot.NoLegs.Value -= newRobot.NoLegs.Value - i;
                break;
            }
            int legIndex = legIndexes[Random.Range(0, legIndexes.Count - 1)];
            if (newRobot.UniformBody.Value)
            {
                for (int spawnIndex = 0; spawnIndex < NoSpawnPoints; spawnIndex++)
                {
                    LegConfig newLeg = newRobot.CreateLeg(index, Mathf.FloorToInt(legIndex), spawnIndex);
                    newRobot.AverageRestOfLegs(newLeg);
                    i++;
                }
            }
            else
            {
                LegConfig newLeg = newRobot.CreateLeg(index, Mathf.FloorToInt(legIndex / NoSpawnPoints), legIndex % NoSpawnPoints);
                newRobot.AverageRestOfLegs(newLeg);
            }
        }
        List<ObjectConfig> actualLegs = newRobot.Configs.Where(o => o.Type == BodyPart.Leg).ToList();
        //are there fewer legs now?
        for (int i = 0; i < actualLegs.Count() - newRobot.NoLegs.Value; i++)
        {
            ObjectConfig leg = null;
            try { leg = actualLegs.Last(); }
            catch (Exception ex) { 
                GameController.Controller.SingleRespawn(ex.ToString(), newRobot); return; 
            }
            newRobot.RemoveLeg(leg);
        }

        if (newRobot.UniformBody.Value && !oldRobot.UniformBody.Value) newRobot.MakeBodyUniform();

        //update existing configs
        foreach (ObjectConfig item in newRobot.Configs)
        {
            if(item.Type == BodyPart.Body)
            {
                newRobot.UpdateBodyPart(item, BodyPart.Body);
            }
            else if(item.Type == BodyPart.Tail)
            {
                newRobot.UpdateBodyPart(item, BodyPart.Tail);
            }
            else if(item.Type == BodyPart.Leg)
            {
                newRobot.UpdateBodyPart(item, BodyPart.Leg);
            }
        }

        if (newRobot.MaintainSerpentine.Value) newRobot.MakeSerpentine(false);
        //if the robot camera is following this robot then update its Head & Tail variables
        if (CameraConfig.CamFollow == newRobot.RobotIndex && CameraConfig.RobotCamera.activeSelf == true) CameraConfig.RobotCamera.GetComponent<CameraPosition>().SetRobot(newRobot);
    }

    private static RobotConfig Init(RobotConfig newRobot, RobotConfig oldRobot, int newVersion)
    {
        newRobot.FreshCopy(oldRobot, newVersion, oldRobot.RobotIndex);
        //setup ready to respawn
        newRobot.Object.name = $"Robot {newRobot.RobotIndex + 1} V {newRobot.Version}";
        newRobot.Object.transform.parent = oldRobot.Object.transform.parent;

        //replace stuck robot with new robot in RobotConfigs
        int index = -1;
        try { index = AIConfig.RobotConfigs.IndexOf(AIConfig.RobotConfigs.First(c => c.RobotIndex.Equals(newRobot.RobotIndex))); }
        catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return newRobot; }    
        AIConfig.RobotConfigs[index] = newRobot;

        //fill RobotConfig.Configs with the ObjectConfigs of each body part
        foreach (Transform item in newRobot.Object.transform)
        {
            ObjectConfig objConfig = item.gameObject.GetComponent<ObjectConfig>();
            if (objConfig != null)
            {
                ObjectConfig oldObjConfig = null;
                try {
                    oldObjConfig = oldRobot.Configs.First(o => o.Type == objConfig.Type && o.Index == objConfig.Index);
                }
                catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), oldRobot); return newRobot; }
                
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
                else if(objConfig.Type == BodyPart.Leg)
                {
                    objConfig.Leg = new LegConfig(oldObjConfig.Leg.AttachedBody, (int)oldObjConfig.Leg.Position);
                    objConfig.Leg.Clone(oldObjConfig.Leg);
                }
                newRobot.Configs.Add(objConfig);
            }
        }
        return newRobot;
    }
}
