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
        try
        {
            CameraConfig.Hat.transform.parent = newRobot.Configs.First(b => b.Type == BodyPart.Body && b.Index == 0).transform;
            CameraConfig.Hat.transform.localPosition = new Vector3(0, 0.3f, -0.15f);
            CameraConfig.Hat.transform.localRotation = Quaternion.Euler(-10, 0, 0);
        }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
    }

    private void UpdateHeadTail()
    {
        //validate objects have been organised correctly
        GameObject head, tail = robot.Object, back;
        //all body parts
        List<ObjectConfig> Sections = robot.Configs.Where(o => o.Type == BodyPart.Body).ToList();
        if (Sections.Count != robot.NoSections.Value) throw new Exception($"There are {Sections.Count} sections set up where there should be {robot.NoSections.Value}.");
        try { 
            head = Sections.First(o => o.Index == 0).gameObject; //head
            if (robot.IsTailEnabled.Value) tail = robot.Configs.First(o => o.Type == BodyPart.Tail).gameObject; //tail
            back = Sections.First(o => o.Index == robot.NoSections.Value - 1).gameObject; //last section
            //give camera the objects it needs
            Head = head;
            Tail = robot.IsTailEnabled.Value ? tail : back;
        }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
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
