using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEditor;

public class GenerateRobot : MonoBehaviour
{
    private RobotConfig robotConfig;
    private int layer;
    private RobotConfig oldRobot;

    void Awake()
    {
        //setup robot config
        robotConfig = new RobotConfig(AIConfig.RobotConfigs.Count, this.gameObject);
        if (DebugConfig.InitRobots.Count > 0 && DebugConfig.IsTotalRespawn) //total respawn
        {
            //if this is a total respawn then start the robot as it left off
            oldRobot = DebugConfig.InitRobots[AIConfig.RobotConfigs.Count];
            robotConfig.Clone(oldRobot);
            AIConfig.RobotConfigs.Add(robotConfig);
        }
        else if(DebugConfig.InitRobots.Count > 0) //single respawn
        {
            oldRobot = DebugConfig.InitRobots.First();
            robotConfig.Clone(oldRobot);
            int index = AIConfig.RobotConfigs.IndexOf(oldRobot);
            if(index < 0)
            {
                //respawn has been called 2+ times in quick succession before the method had a chance to disable the scripts
                //multiple robots have been created - currently just an empty gameobject
                //delete and move on
                Destroy(this.gameObject);
                return;
            }
            AIConfig.RobotConfigs[index] = robotConfig;
            DebugConfig.InitRobots.Remove(oldRobot);
        }
        else //new robot
        {
            robotConfig.Original = robotConfig;
            AIConfig.RobotConfigs.Add(robotConfig);
        }

        //setup overall robot
        GameObject robot = this.gameObject;
        robot.name = $"Robot {robotConfig.RobotIndex + 1} V {robotConfig.Version}";
        Vector3 spawnPoint = TerrainConfig.GetSpawnPoint(robotConfig.RobotIndex);
        robot.transform.position = new Vector3(spawnPoint.x, robotConfig.GetYPos(), spawnPoint.z);

        //get layer for this robot
        layer = LayerMask.NameToLayer($"Robot{(robotConfig.RobotIndex % 25) + 1}");
        robot.layer = layer;

        SetupBody(robot);

        robotConfig.ValidateLegParams();
        SetupLegs(robot);

        if (robotConfig.IsTailEnabled.Value)
        {
            ObjectConfig tail;
            try { tail = oldRobot.Configs.First(o => o.Type == BodyPart.Tail);  }
            catch (Exception) { tail = null; }
            robotConfig.CreateTail(tail);
        }

        robotConfig.SetChildLayer(layer);

        if (robotConfig.UniformBody.Value) robotConfig.MakeBodyUniform();

        //if old robot exists - this is a respawn - then that can now be deleted
        if (oldRobot != null)
        {
            if (DebugConfig.IsTotalRespawn) Destroy(oldRobot.Object.transform.parent.gameObject);
            else Destroy(oldRobot.Object.gameObject);
        }

        //add dynamic movement script if being used
        if (DynMovConfig.UseDynamicMovement)
        {
            DynamicMovement dynmov = robot.GetComponent<DynamicMovement>();
            if (dynmov == null) dynmov = robot.AddComponent<DynamicMovement>();
        }

        //destroy GenerateRobot so that a duplicate clone isn't created when this robot is cloned
        Destroy(this);
    }

    private void SetupBody(GameObject robot)
    {
        //initialise with random colour
        if(oldRobot == null) robotConfig.BodyColour.Value = Random.Range(30, 70);
        for (int i = 0; i < robotConfig.NoSections.Value; i++)
        {
            ObjectConfig body;
            try { body = oldRobot.Configs.First(o => o.Type == BodyPart.Body && o.Index == i); }
            catch (Exception) { body = null; }
            if (i == 0) robotConfig.CreateHead(body);
            else robotConfig.CreateBody(i, body);
        }
        if (robotConfig.MaintainSerpentine.Value && oldRobot == null) robotConfig.MakeSerpentine(!AIConfig.RandomInitValues);
    }

    private void SetupLegs(GameObject robot)
    {
        int NoSpawnPoints = 2;
        List<int> legIndexes = robotConfig.UniformBody.Value ? Enumerable.Range(1, (int)robotConfig.NoSections.Value - 1).ToList() : Enumerable.Range(2, (int)(robotConfig.NoSections.Value - 1) * 2).ToList();
        //e.g. 3 sections
        //uniform -> [1, 2]
        //nonuniform -> [2, 3, 4, 5]
        for (int i = 0; i < robotConfig.NoLegs.Value; i++)
        {
            if (legIndexes.Count == 0)
            {
                //weird bug here where sometimes the no legs is 1 when it should be zero
                //haven't had time to track it down - doesn't seem to be an issue with ValidateParams
                robotConfig.NoLegs.Value -= robotConfig.NoLegs.Value - i;
                break;
            }
            int index = legIndexes[Random.Range(0, legIndexes.Count - 1)];
            legIndexes.Remove(index);
            if (robotConfig.UniformBody.Value)
            {
                for (int spawnIndex = 0; spawnIndex < NoSpawnPoints; spawnIndex++)
                {
                    ObjectConfig leg;
                    try { leg = oldRobot.Configs.First(o => o.Type == BodyPart.Leg && o.Index == i); }
                    catch (Exception) { leg = null; }
                    robotConfig.CreateLeg(i, Mathf.FloorToInt(index), spawnIndex, leg); 
                    i++;
                }
            }
            else
            {
                ObjectConfig leg;
                try { leg = oldRobot.Configs.First(o => o.Type == BodyPart.Leg && o.Index == i); }
                catch (Exception) { leg = null; }
                robotConfig.CreateLeg(i, Mathf.FloorToInt(index / NoSpawnPoints), index % NoSpawnPoints, leg);
            }
        }
    }







}