using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class GenerateRobot : MonoBehaviour
{
    private RobotConfig robotConfig;
    private int layer;
    private RobotConfig oldRobot;

    void Awake()
    {
        //setup robot config
        robotConfig = new RobotConfig(AIConfig.RobotConfigs.Count, this.gameObject);
        if (AIConfig.InitRobots.Count == AIConfig.PopulationSize)
        {
            //if this is a total respawn then start the robot as it left off
            oldRobot = AIConfig.InitRobots[AIConfig.RobotConfigs.Count];
            robotConfig.Clone(oldRobot);
            AIConfig.RobotConfigs.Add(robotConfig);
        }
        else if(AIConfig.InitRobots.Count > 0 && AIConfig.InitRobots.Count < AIConfig.PopulationSize)
        {
            oldRobot = AIConfig.InitRobots.First();
            robotConfig.Clone(oldRobot);
            AIConfig.RobotConfigs[AIConfig.RobotConfigs.IndexOf(oldRobot)] = robotConfig;
            AIConfig.InitRobots.Remove(oldRobot);
        }
        else
        {
            robotConfig.Original = robotConfig;
            AIConfig.RobotConfigs.Add(robotConfig);
        }

        //setup overall robot
        GameObject robot = this.gameObject;
        string prefix = oldRobot == null ? "" : "NEW";
        robot.name = $"{prefix} Robot {robotConfig.RobotIndex + 1} V {robotConfig.Version}";
        Vector3 spawnPoint = AIConfig.SpawnPoints[Mathf.FloorToInt(robotConfig.RobotIndex / 25)];
        robot.transform.position = new Vector3(spawnPoint.x, robotConfig.GetYPos(), spawnPoint.z);

        //get layer for this robot
        layer = LayerMask.NameToLayer($"Robot{(robotConfig.RobotIndex % 25) + 1}");
        robot.layer = layer;

        SetupBody(robot);

        //TODO: setup legs

        if (robotConfig.IsTailEnabled.Value)
        {
            ObjectConfig tail;
            try { tail = oldRobot.Configs.First(o => o.Type == BodyPart.Tail);  }
            catch (Exception) { tail = null; }
            robotConfig.CreateTail(tail);
        }

        robotConfig.SetChildLayer(layer);

        //if old robot exists - this is a respawn - then that can now be deleted
        if(oldRobot != null) Destroy(oldRobot.Object.gameObject);

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







}