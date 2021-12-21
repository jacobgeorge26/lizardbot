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
        robot.transform.position = new Vector3(0, 10, 0);

        if (BaseConfig.IsDefault) DefaultParams();
        else if (!ValidateParams()) return;

        SetupBody(robot);

        //TODO: setup legs

        SetupCam(robot);
    }


    private void DefaultParams()
    {
        BaseConfig.SectionConfigs.Clear();
        BaseConfig.NoSections = BaseConfig.NoSections == -1 ? BaseConfig.DefaultNoSections : BaseConfig.NoSections;
        for (int i = 0; i < BaseConfig.NoSections; i++)
        {
            //BodyConfig comes with default params - exceptions are shown below - change from default every other section
            BodyConfig config = new BodyConfig();
            ////rotation defaults
            config.IsRotating = i % 2 == 0 ? true : false;
            
            BaseConfig.SectionConfigs.Add(config);
        }
    }

    private bool ValidateParams()
    {
        if(BaseConfig.NoSections < 1)
        {
            Debug.LogError("The minimum number of body sections is 1");
            return false;
        }
        if (BaseConfig.SectionConfigs.Count != BaseConfig.NoSections)
        {
            Debug.LogError("The number of section configs in BaseConfig must equal the number of sections");
            return false;
        }
        for (int i = 0; i < BaseConfig.SectionConfigs.Count; i++)
        {
            BodyConfig config = BaseConfig.SectionConfigs[i];
            if (config.DriveVelocity < 0.1f)
            {
                Debug.LogWarning($"Section {i + 1} will not drive due to a drive velocity of {config.DriveVelocity}");
            }
            for (int a = 0; a < config.AngleConstraint.Length; a++)
            {
                string angle = a switch
                {
                    0 => "x",
                    1 => "y",
                    2 => "z",
                    _ => "unknown"
                };
                if(config.RotationMultiplier[a] < 0.1)
                {
                    Debug.LogWarning($"The {angle} turn ration of section {i + 1} will not rotate due to a turn ratio of {config.RotationMultiplier[a]}");
                }
                if(config.AngleConstraint[a] < 30)
                {
                    Debug.LogWarning($"The {angle} angle constraint of section {i + 1} is {config.AngleConstraint[a]} which may produce unstable results. An angle >30 is recommended");
                }
                if(config.AngleConstraint[a] > 120)
                {
                    Debug.LogWarning($"The {angle} angle constraint of section {i + 1} is {config.AngleConstraint[a]} which may produce unstable results. An angle <120 is recommended");
                }
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
            BodyConfig newConfig = BaseConfig.SectionConfigs[i];
            //index
            config.Index = i;
            //driving
            config.IsDriving = newConfig.IsDriving;
            config.DriveVelocity = newConfig.DriveVelocity;
            //rotating
            config.IsRotating = newConfig.IsRotating;
            config.AngleConstraint = newConfig.AngleConstraint;
            config.RotationMultiplier = newConfig.RotationMultiplier;
            
            //setup configurable joints
            if (i > 0)
            {
                ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>(); //TODO: make the angle constraints dynamic
                joint.lowAngularXLimit = new SoftJointLimit() { limit = -1 * config.AngleConstraint[0] / 2 };
                joint.highAngularXLimit = new SoftJointLimit() { limit = config.AngleConstraint[0] / 2 };
                joint.angularYLimit = new SoftJointLimit() { limit = config.AngleConstraint[1] / 2 };
                joint.angularZLimit = new SoftJointLimit() { limit = config.AngleConstraint[2] / 2 };
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
