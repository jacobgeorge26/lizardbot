using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

public class GenerateRobot : BaseConfig
{
    void Start()
    {
        //setup overall robot
        GameObject robot = new GameObject();
        robot.name = "robot";
        robot.transform.position = new Vector3(0, 3, -8);

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
            config.MaxAngle = MaxAngle; //TODO: make this better dynamic
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

        //setup camera
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.transform.parent = robot.transform;
        CameraPosition camPos = cam.GetComponent<CameraPosition>();
        camPos.Head = Sections[0];
        camPos.Tail = Sections[Sections.Count - 1];
    }
}
