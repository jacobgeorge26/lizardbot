using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;
using Random = System.Random;

public class GenerateRobot : MonoBehaviour
{
    private RobotConfig robotConfig;
    private RobotHelpers helpers;
    private int layer;

    void Start()
    {
        //setup robot config
        robotConfig = this.gameObject.GetComponent<RobotConfig>();
        if (robotConfig == null) robotConfig = this.gameObject.AddComponent<RobotConfig>();
        robotConfig.RobotIndex = AIConfig.RobotConfigs.Count;
        robotConfig.Original = robotConfig;
        AIConfig.RobotConfigs.Add(robotConfig);

        //setup overall robot
        GameObject robot = this.gameObject;
        robot.name = $"Robot {robotConfig.RobotIndex + 1} V {robotConfig.Version}";
        helpers = robot.GetComponent<RobotHelpers>();
        if (helpers == null) helpers = robot.AddComponent<RobotHelpers>();
        robot.transform.position = new Vector3(0, helpers.GetYPos(), 0);

        //get layer for this robot
        layer = LayerMask.NameToLayer($"Robot{(robotConfig.RobotIndex % 25) + 1}");
        robot.layer = layer;

        //important - give helpers the info it needs
        helpers.Init(robot, robotConfig);

        SetupBody(robot);

        //TODO: setup legs

        if (robotConfig.IsTailEnabled.Value) helpers.CreateTail();

        helpers.SetChildLayer(layer);

        //destroy GenerateRobot so that a duplicate clone isn't created when this robot is cloned
        Destroy(this);
    }

    private void SetupBody(GameObject robot)
    {
        List<BodyConfig> rotatingSections = new List<BodyConfig>();
        //initialise with random colour
        robotConfig.BodyColour.Value = new Random().Next(30, 70);
        for (int i = 0; i < robotConfig.NoSections.Value; i++)
        {
            if (i == 0) helpers.CreateHead();
            else helpers.CreateBody(i);
        }
        if (AIConfig.MaintainSerpentine) helpers.MakeSerpentine(!AIConfig.RandomInitValues);
    }







}