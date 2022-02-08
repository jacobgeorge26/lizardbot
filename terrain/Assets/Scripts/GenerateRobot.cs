using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;
using System;
using System.Linq;

public class GenerateRobot : MonoBehaviour
{
    private RobotConfig robotConfig;
    private int layer;

    void Start()
    {
        //setup robot config
        robotConfig = this.gameObject.GetComponent<RobotConfig>();
        if (robotConfig == null) robotConfig = this.gameObject.AddComponent<RobotConfig>();
        robotConfig.RobotIndex = AIConfig.RobotConfigs.Count;
        AIConfig.RobotConfigs.Add(robotConfig);

        //setup overall robot
        GameObject robot = this.gameObject;
        robot.name = $"robot{robotConfig.RobotIndex + 1}";
        robot.transform.position = new Vector3(0, TerrainConfig.GetTerrainHeight() + 1f, 0);

        //get layer for this robot
        layer = LayerMask.NameToLayer($"Robot{robotConfig.RobotIndex + 1}");

        SetupBody(robot);

        //TODO: setup legs

        if(robotConfig.IsTailEnabled.Value) SetupTail(robot);

        if(robotConfig.RobotIndex == 0) SetupCam(robot);
    }  

    private void SetupBody(GameObject robot)
    {
        List<BodyConfig> rotatingSections = new List<BodyConfig>();
        for (int i = 0; i < robotConfig.NoSections.Value; i++)
        {
            GameObject section = i == 0
                ? MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"))
                : MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
            GameObject prevSection = i == 0 ? null : robotConfig.Configs.Where(o => o.Type == BodyPart.Body && o.Index == i - 1).First().Object;
            section.name = i == 0 ? "head" : $"section{i}";
            section.transform.parent = robot.transform;
            section.transform.localPosition = new Vector3(0, 0, GetZPos(i == 0 ? null : prevSection, section));

            //set layer
            section.layer = layer;

            //setup BodyConfig for MoveBody script
            BodyConfig config = section.GetComponent<BodyConfig>();
            if (config == null) config = section.AddComponent<BodyConfig>();
            //setup how rotation will work for this section
            config.IsRotating.Value = i % 2 == 0 ? true : false;
            if (config.IsRotating.Value) rotatingSections.Add(config);

            ObjectConfig objConfig = section.GetComponent<ObjectConfig>();
            if (objConfig == null) objConfig = section.AddComponent<ObjectConfig>();
            //important!!
            objConfig.Init(i, BodyPart.Body, section, robotConfig.RobotIndex);
            robotConfig.Configs.Add(objConfig);
            
            //setup configurable joints
            if (i > 0)
            {
                SetupConfigurableJoint(section, config, prevSection);
            }
        }
        //alternate betwwen sin and cos for rotating sections
        for (int i = 0; i < rotatingSections.Count; i++)
        {
            rotatingSections[i].UseSin.Value = i % 2 == 0 ? true : false;
        }
    }

    private void SetupTail(GameObject robot)
    {
        GameObject tail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Tail"));
        tail.name = "tail";
        tail.transform.parent = robot.transform;
        GameObject lastSection = robotConfig.Configs.Where(o => o.Type == BodyPart.Body && o.Index == robotConfig.NoSections.Value - 1).First().Object;
        tail.transform.localPosition = new Vector3(0, 0, GetZPos(lastSection, tail));

        //set layer
        tail.layer = layer;

        TailConfig config = tail.GetComponent<TailConfig>();
        if (config == null) config = tail.AddComponent<TailConfig>();
        //setup config
        config.AngleConstraint = new RangedVariable(new Vector3(60, 60, 60), 0, 180);
        config.RotationMultiplier = new RangedVariable(new Vector3(1f, 1f, 1f), 0.5f, 1f);

        //set mass to be equal to rest of body
        tail.GetComponent<Rigidbody>().mass = GetTotalMass() * config.TailMassMultiplier.Value; 

        ObjectConfig objConfig = tail.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = tail.AddComponent<ObjectConfig>();


        //important!!
        objConfig.Init(0, BodyPart.Tail, tail, robotConfig.RobotIndex);
        robotConfig.Configs.Add(objConfig);

        //setup joint
        SetupConfigurableJoint(tail, config, lastSection);
    }


    private float GetZPos(GameObject prevObject, GameObject thisObject)
    {
        if(prevObject == null) return 0;
        //determine the position of this section by the location of the prev section, and size of both
        float zPos = prevObject.transform.position.z;
        zPos += -1 * (prevObject.transform.localScale.z / 2 + thisObject.transform.localScale.z / 2 + 0.1f);
        return zPos;
    }

    private float GetTotalMass()
    {
        float sumMass = 0;
        //iterate objects, get sum of (mass * axis coordinate), divide by sum of all masses
        foreach (ObjectConfig objConfig in robotConfig.Configs)
        {
            sumMass += objConfig.Object.GetComponent<Rigidbody>().mass;
        }
        return sumMass;
    }

    private void SetupConfigurableJoint(GameObject section, JointConfig config, GameObject prevObject)
    {
        ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>();
        joint.lowAngularXLimit = new SoftJointLimit() { limit = -1 * config.AngleConstraint.Value[0] / 2 };
        joint.highAngularXLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[0] / 2 };
        joint.angularYLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[1] / 2 };
        joint.angularZLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[2] / 2 };
        joint.connectedBody = prevObject.GetComponent<Rigidbody>();
    }

    private void SetupCam(GameObject robot)
    {
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.transform.parent = robot.transform;
        CameraPosition camPos = cam.GetComponent<CameraPosition>();
        //validate objects have been organised correctly
        GameObject head, tail = robot, back;
        //all body parts
        List<ObjectConfig> Sections = robotConfig.Configs.Where(o => o.Type == BodyPart.Body).ToList();
        if (Sections.Count != robotConfig.NoSections.Value) throw new Exception($"There are {Sections.Count} sections set up where there should be {robotConfig.NoSections.Value}.");
        //head
        List<ObjectConfig> Heads = Sections.Where(o => o.Index == 0).ToList();
        if (Heads.Count > 1) throw new Exception("The indexing has not been initialised correctly or there is more than one head.");
        else head = Heads.First().Object;
        //tail
        List<ObjectConfig> Tails = robotConfig.Configs.Where(o => o.Type == BodyPart.Tail).ToList();
        if (Tails.Count > 1 || (Tails.Count == 1 && !robotConfig.IsTailEnabled.Value)) throw new Exception("There is more than one tail, or a tail exists when IsTailEnabled is set to false.");
        else if(robotConfig.IsTailEnabled.Value) tail = Tails.First().Object;
        //last section
        List<ObjectConfig> Backs = Sections.Where(o => o.Index == robotConfig.NoSections.Value - 1).ToList();
        if (Backs.Count != 1) throw new Exception("The last section either does not exist or has multiple sections with the same index.");
        else back = Backs.First().Object;

        //give camera the objects it needs
        camPos.Head = head;
        camPos.Tail = robotConfig.IsTailEnabled.Value ? tail : back;
    }

}
