using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;

public class GenerateRobot : BaseConfig
{
    [SerializeField]
    [InspectorName("Use Default Configuration")]
    protected bool isDefault;

    void Start()
    {
        //setup overall robot
        GameObject robot = new GameObject();
        robot.name = "robot";
        robot.transform.position = new Vector3(0, 3, -8);

        if (isDefault) DefaultParams();
        else if (!ValidateParams()) return;

        SetupBody(robot);

        //TODO: setup legs

        SetupCam(robot);
    }

    private void DefaultParams()
    {
        NoSections = 5;
        DrivingSections = new bool[5] { true, true, true, true, true };
        RotatingSections = new bool[5] { true, false, true, false, true };
        TurnVelocity = 180;
        DriveVelocity = 1f;
        MaxAngle = new int[3] { 30, 60, 45 };
    }

    private bool ValidateParams()
    {
        if(NoSections < 1)
        {
            Debug.LogError("The minimum number of body sections is 1");
            return false;
        }
        if (RotatingSections.Length != NoSections)
        {
            Debug.LogError("The number of rotating sections must equal the number of sections");
            return false;
        }
        if(DrivingSections.Length != NoSections)
        {
            Debug.LogError("The number of driving sections must equal the number of sections");
            return false;
        }
        //TODO: update once individual customisation is setup - will need to look at each one - drive & turn
        if(TurnVelocity < 1)
        {
            Debug.LogWarning("The rotating sections will not rotate due to the turn velocity");
        }
        if(DriveVelocity < 0.1f)
        {
            Debug.LogWarning("The driving sections will not rotate due to the drive velocity");
        }
        //TODO: add more info about which section and angle it is
        foreach(int angle in MaxAngle)
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
        for (int i = 0; i < NoSections; i++)
        {
            GameObject section = i == 0
                ? MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"))
                : MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
            section.name = i == 0 ? "head" : $"section{i}";
            Sections.Add(section);
            section.transform.parent = robot.transform;
            section.transform.localPosition = new Vector3(0, 0, (float)(i * -1.1));

            //setup BodyConfig for MoveBody script
            BodyConfig config = section.GetComponent<BodyConfig>();
            config.IsRotating = RotatingSections[i];
            config.IsClockwise = new bool[3] { false, true, false }; //TODO: make this dynamic
            config.MaxAngle = MaxAngle;
            config.TurnVelocity = TurnVelocity;
            config.IsDriving = DrivingSections[i];
            config.DriveVelocity = DriveVelocity;

            //setup configurable joints
            if (i > 0)
            {
                ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>(); //TODO: make the angle constraints dynamic
                joint.connectedBody = Sections[i - 1].GetComponent<Rigidbody>();
            }
        }
    }

    private void SetupCam(GameObject robot)
    {
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.transform.parent = robot.transform;
        CameraPosition camPos = cam.GetComponent<CameraPosition>();
        camPos.Head = Sections[0];
        camPos.Tail = Sections[Sections.Count - 1];
    }
}
