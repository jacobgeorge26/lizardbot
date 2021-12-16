using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;

public class GenerateRobot : MonoBehaviour
{
    void Start()
    {
        //setup overall robot
        GameObject robot = new GameObject();
        robot.name = "robot";
        robot.transform.position = new Vector3(0, 3, 0);

        if (BaseConfig.isDefault) DefaultParams();
        else if (!ValidateParams()) return;

        SetupBody(robot);

        //TODO: setup legs

        SetupCam(robot);
    }


    private void DefaultParams()
    {
        BaseConfig.NoSections = BaseConfig.NoSections == -1 ? BaseConfig.DefaultNoSections : BaseConfig.NoSections;
        BaseConfig.DrivingSections = new bool[BaseConfig.NoSections];
        BaseConfig.RotatingSections = new bool[BaseConfig.NoSections];
        for (int i = 0; i < BaseConfig.NoSections; i++)
        {
            BaseConfig.DrivingSections[i] = true;
            BaseConfig.RotatingSections[i] = i % 2 == 0 ? true : false;
        }
        BaseConfig.TurnVelocity = 180;
        BaseConfig.DriveVelocity = 1f;
        BaseConfig.MaxAngle = new int[3] { 30, 60, 45 };
    }

    private bool ValidateParams()
    {
        if(BaseConfig.NoSections < 1)
        {
            Debug.LogError("The minimum number of body sections is 1");
            return false;
        }
        if (BaseConfig.RotatingSections.Length != BaseConfig.NoSections)
        {
            Debug.LogError("The number of rotating sections must equal the number of sections");
            return false;
        }
        if(BaseConfig.DrivingSections.Length != BaseConfig.NoSections)
        {
            Debug.LogError("The number of driving sections must equal the number of sections");
            return false;
        }
        //TODO: update once individual customisation is setup - will need to look at each one - drive & turn
        if(BaseConfig.TurnVelocity < 1)
        {
            Debug.LogWarning("The rotating sections will not rotate due to the turn velocity");
        }
        if(BaseConfig.DriveVelocity < 0.1f)
        {
            Debug.LogWarning("The driving sections will not rotate due to the drive velocity");
        }
        //TODO: add more info about which section and angle it is
        foreach(int angle in BaseConfig.MaxAngle)
        {
            if(angle > 60)
            {
                Debug.LogWarning("A maximum angle is >60 which may produce unstable results");
            }
        }
        return true;
    }

    private void SetupBody(GameObject robot)
    {
        for (int i = 0; i < BaseConfig.NoSections; i++)
        {
            GameObject section = i == 0
                ? MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"))
                : MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
            section.name = i == 0 ? "head" : $"section{i}";
            BaseConfig.Sections.Add(section);
            section.transform.parent = robot.transform;
            section.transform.localPosition = new Vector3(0, 0, (float)(i * -1.1));

            //setup BodyConfig for MoveBody script
            BodyConfig config = section.GetComponent<BodyConfig>();
            config.IsRotating = BaseConfig.RotatingSections[i];
            config.IsClockwise = new bool[3] { false, true, false }; //TODO: make this dynamic
            config.MaxAngle = BaseConfig.MaxAngle;
            config.TurnVelocity = BaseConfig.TurnVelocity;
            config.IsDriving = BaseConfig.DrivingSections[i];
            config.DriveVelocity = BaseConfig.DriveVelocity;

            //setup configurable joints
            if (i > 0)
            {
                ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>(); //TODO: make the angle constraints dynamic
                joint.connectedBody = BaseConfig.Sections[i - 1].GetComponent<Rigidbody>();
            }
        }
    }

    private void SetupCam(GameObject robot)
    {
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.transform.parent = robot.transform;
        CameraPosition camPos = cam.GetComponent<CameraPosition>();
        camPos.Head = BaseConfig.Sections[0];
        camPos.Tail = BaseConfig.Sections[BaseConfig.Sections.Count - 1];
    }

}
