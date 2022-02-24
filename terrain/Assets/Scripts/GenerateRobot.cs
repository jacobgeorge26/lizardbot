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

    void Awake()
    {
        //setup robot config
        robotConfig = new RobotConfig(AIConfig.RobotConfigs.Count, this.gameObject);

        robotConfig.Original = robotConfig;
        AIConfig.RobotConfigs.Add(robotConfig);

        //setup overall robot
        GameObject robot = this.gameObject;
        robot.name = $"Robot {robotConfig.RobotIndex + 1} V {robotConfig.Version}";
        robot.transform.position = new Vector3(0, robotConfig.GetYPos(), 0);

        //get layer for this robot
        layer = LayerMask.NameToLayer($"Robot{(robotConfig.RobotIndex % 25) + 1}");
        robot.layer = layer;

        SetupBody(robot);

        //TODO: setup legs

        if (robotConfig.IsTailEnabled.Value) robotConfig.CreateTail();

        robotConfig.SetChildLayer(layer);

        //give UI this robot if this is the one the cam is following
        //if (CameraConfig.CamFollow == robotConfig.RobotIndex) FindObjectOfType<UI>().UpdateRobotUI(robotConfig);

        //destroy GenerateRobot so that a duplicate clone isn't created when this robot is cloned
        Destroy(this);
    }

    private void SetupBody(GameObject robot)
    {
        List<BodyConfig> rotatingSections = new List<BodyConfig>();
        //initialise with random colour
        robotConfig.BodyColour.Value = Random.Range(30, 70);
        for (int i = 0; i < robotConfig.NoSections.Value; i++)
        {
            if (i == 0) robotConfig.CreateHead();
            else robotConfig.CreateBody(i);
        }
        if (robotConfig.MaintainSerpentine.Value) robotConfig.MakeSerpentine(!AIConfig.RandomInitValues);
    }







}