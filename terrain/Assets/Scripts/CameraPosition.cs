using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    public GameObject Head;

    public GameObject Tail;

    private RobotConfig robot;

    private float offset = 5;

    //when a new robot is set as the one to follow, the script is added
    public void SetRobot(RobotConfig newRobot)
    {
        robot = newRobot;
        UpdateHeadTail();
        List<ObjectConfig> heads = newRobot.Configs.Where(b => b.Type == BodyPart.Body && b.Index == 0).ToList();
        if (heads.Count != 1) throw new Exception($"There was an issue selecting Robot {robot} as the active robot: invalid number of heads found.");
        else
        {
            CameraConfig.Hat.transform.parent = heads.First().transform;
            CameraConfig.Hat.transform.localPosition = new Vector3(0, 0.3f, -0.15f);
            CameraConfig.Hat.transform.localRotation = Quaternion.Euler(-10, 0, 0);
        }
    }

    private void UpdateHeadTail()
    {
        //validate objects have been organised correctly
        GameObject head, tail = robot.Object, back;
        //all body parts
        List<ObjectConfig> Sections = robot.Configs.Where(o => o.Type == BodyPart.Body).ToList();
        if (Sections.Count != robot.NoSections.Value) throw new Exception($"There are {Sections.Count} sections set up where there should be {robot.NoSections.Value}.");
        //head
        List<ObjectConfig> Heads = Sections.Where(o => o.Index == 0).ToList();
        if (Heads.Count > 1) throw new Exception("The indexing has not been initialised correctly or there is more than one head.");
        else head = Heads.First().gameObject;
        //tail
        List<ObjectConfig> Tails = robot.Configs.Where(o => o.Type == BodyPart.Tail).ToList();
        if (Tails.Count > 1 || (Tails.Count == 1 && !robot.IsTailEnabled.Value)) throw new Exception("There is more than one tail, or a tail exists when IsTailEnabled is set to false.");
        else if (robot.IsTailEnabled.Value) tail = Tails.First().gameObject;
        //last section
        List<ObjectConfig> Backs = Sections.Where(o => o.Index == robot.NoSections.Value - 1).ToList();
        if (Backs.Count != 1) throw new Exception("The last section either does not exist or has multiple sections with the same index.");
        else back = Backs.First().gameObject;

        //give camera the objects it needs
        Head = head;
        Tail = robot.IsTailEnabled.Value ? tail : back;
    }



    // Update is called once per frame
    void Update()
    {
        if(robot != null)
        {
            CameraConfig.RobotCamera.transform.position = GetCameraPosition();
        }
    }

    public void Clear()
    {
        robot = null;
    }

    public Vector3 GetCameraPosition()
    {
        Vector3 newPos = new Vector3();
        newPos.x = (Head.transform.position.x + Tail.transform.position.x) / 2;
        newPos.y = (Head.transform.position.y + Tail.transform.position.y) / 2 + offset;
        newPos.z = (Head.transform.position.z + Tail.transform.position.z) / 2 - (offset / 2);
        return newPos;
    }
}
