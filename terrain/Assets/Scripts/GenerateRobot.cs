using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;

public class GenerateRobot : MonoBehaviour
{
    void Start()
    {
        //setup overall robot
        GameObject robot = new GameObject();
        robot.name = "robot";
        robot.transform.position = new Vector3(0, 10, 0);

        SetupBody(robot);

        if (BaseConfig.IsDefault) DefaultParams();
        else if (!ValidateParams()) return;

        //TODO: setup legs

        SetupTail(robot);

        SetupCam(robot);
    }

    private void DefaultParams()
    {
        BaseConfig.SectionConfigs.Clear();
        for (int i = 0; i < BaseConfig.NoSections; i++)
        {
            //BodyConfig comes with default params - exceptions are shown below - change from default every other section
            BodyConfig config = BaseConfig.Sections[i].GetComponent<MoveBody>().GetBodyConfig(); ;
            //rotation defaults
            config.Index = i;
            //////////////////////////////////////////config.IsRotating = i % 2 == 0 ? true : false;
            config.IsRotating = false;
            
            BaseConfig.SectionConfigs.Add(config);
        }

        //alternate betwwen sin and cos
        List<BodyConfig> rotatingSections = BaseConfig.SectionConfigs.Where(s => s.IsRotating).ToList();
        for (int i = 0; i < rotatingSections.Count; i++)
        {
            rotatingSections[i].UseSin = i % 2 == 0 ? true : false;
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
            for (int a = 0; a < 2; a++)
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
        BaseConfig.NoSections = BaseConfig.NoSections == -1 ? BaseConfig.DefaultNoSections : BaseConfig.NoSections;
        for (int i = 0; i < BaseConfig.NoSections; i++)
        {
            GameObject section = i == 0
                ? MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"))
                : MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
            section.name = i == 0 ? "head" : $"section{i}";
            BaseConfig.Sections.Add(section);
            section.transform.parent = robot.transform;
            section.transform.localPosition = new Vector3(0, 0, GetZPos(i == 0 ? null : BaseConfig.Sections[i - 1], section));

            //setup BodyConfig for MoveBody script - needs to have the baseconfig variables copied to it
            BodyConfig config = section.GetComponent<MoveBody>().GetBodyConfig();
            if(BaseConfig.SectionConfigs.Count > 0) BodyConfig.Copy(config, BaseConfig.SectionConfigs[i]);
            //index
            config.Index = i;
            
            //setup configurable joints
            if (i > 0)
            {
                SetupConfigurableJoint(section, config, BaseConfig.Sections[i - 1]);
            }
        }
    }

    private void SetupTail(GameObject robot)
    {
        GameObject tail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Tail"));
        tail.name = "tail";
        BaseConfig.Tail = tail;
        tail.transform.parent = robot.transform;
        tail.transform.localPosition = new Vector3(0, 0, GetZPos(BaseConfig.Sections.Last(), tail));
        TailConfig config = tail.GetComponent<TailConfig>();
        //apply equal force to each axis
        config.RotationMultiplier = new Vector3(1, 1, 1);
        SetupConfigurableJoint(tail, config, BaseConfig.Sections.Last());
        //pass MoveTail the initial centre of gravity (defaulting to zero for some axis)
        tail.GetComponent<MoveTail>().SetInitCOG(GetInitCOG());
    }

    private float GetZPos(GameObject prevObject, GameObject thisObject)
    {
        if(prevObject == null) return 0;
        //determine the position of this section by the location of the prev section, and size of both
        float zPos = prevObject.transform.position.z;
        zPos += -1 * (prevObject.transform.localScale.z / 2 + thisObject.transform.localScale.z / 2 + 0.1f);
        return zPos;
    }

    private void SetupConfigurableJoint(GameObject section, JointConfig config, GameObject prevObject)
    {
        ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>();
        joint.lowAngularXLimit = new SoftJointLimit() { limit = -1 * config.AngleConstraint[0] / 2 };
        joint.highAngularXLimit = new SoftJointLimit() { limit = config.AngleConstraint[0] / 2 };
        joint.angularYLimit = new SoftJointLimit() { limit = config.AngleConstraint[1] / 2 };
        joint.angularZLimit = new SoftJointLimit() { limit = config.AngleConstraint[2] / 2 };
        joint.connectedBody = prevObject.GetComponent<Rigidbody>();
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

    private Vector3 GetInitCOG()
    {
        Vector3 sumMassByPos = new Vector3();
        float sumMass = 0;
        //iterate objects, get sum of (mass * axis coordinate), divide by sum of all masses
        foreach (GameObject section in BaseConfig.Sections)
        {
            float mass = section.GetComponent<Rigidbody>().mass;
            sumMass += mass;
            //x & y default to 0, z needs calculating
            sumMassByPos.z += mass * section.transform.position.z;
        }
        return sumMassByPos / sumMass;
    }

}
