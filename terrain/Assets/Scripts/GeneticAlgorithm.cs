using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void RobotIsStuck(RobotConfig robot)
    {
        Debug.Log($"Robot {robot.RobotIndex} is stuck");
        //destroy child objects - actual robot
        foreach (Transform child in robot.gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        robot.Version++;
        Mutate(robot);

        //rebuild robot
        GenerateRobot builder = robot.gameObject.GetComponent<GenerateRobot>();
        builder.CreateRobot();

        ////generate new robot
        ////GameObject newObject = new GameObject();
        ////RobotConfig newRobot = newObject.AddComponent<RobotConfig>();
        ////newRobot.Copy(robot);

        ////respawn
        ////newObject.AddComponent<GenerateRobot>();

        ////delete old robot
        //Destroy(robot.gameObject);
    }

    private void Mutate(RobotConfig robot)
    {
        List<ObjectConfig> sections = robot.Configs.Where(c => c.Type == BodyPart.Body).ToList();
        sections.ForEach(c =>
            c.Object.GetComponent<BodyConfig>().IsRotating.Value = false
        );
    }
}
