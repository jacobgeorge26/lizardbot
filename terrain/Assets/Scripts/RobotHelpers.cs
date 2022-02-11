using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class RobotHelpers : MonoBehaviour
{
    private GameObject robot;
    private RobotConfig robotConfig;

    internal void Init(GameObject _robot, RobotConfig _robotConfig)
    {
        robot = _robot;
        robotConfig = _robotConfig;
    }

    internal void CreateHead()
    {
        GameObject head = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"));
        head.name = "head";
        head.transform.parent = robot.transform;
        head.transform.localPosition = new Vector3(0, 0, GetZPos(null, head));

        //setup BodyConfig for MoveBody script
        BodyConfig config = head.GetComponent<BodyConfig>();
        if (config == null) config = head.AddComponent<BodyConfig>();

        ObjectConfig objConfig = head.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = head.AddComponent<ObjectConfig>();
        //important!!
        objConfig.Init(0, BodyPart.Body, head, robotConfig.RobotIndex);
        robotConfig.Configs.Add(objConfig);

        UpdateBodyPart(config, 0, BodyPart.Body);
    }

    internal void CreateBody(int index)
    {
        GameObject body = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        GameObject prevSection = robotConfig.Configs.Where(o => o.Type == BodyPart.Body && o.Index == index - 1).First().Object;
        body.name = $"section{index}";
        body.transform.parent = robot.transform;
        body.transform.localPosition = new Vector3(0, 0, GetZPos(prevSection, body));

        //setup BodyConfig for MoveBody script
        BodyConfig config = body.GetComponent<BodyConfig>();
        if (config == null) config = body.AddComponent<BodyConfig>();

        ObjectConfig objConfig = body.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = body.AddComponent<ObjectConfig>();
        //important!!
        objConfig.Init(index, BodyPart.Body, body, robotConfig.RobotIndex);
        robotConfig.Configs.Add(objConfig);

        UpdateBodyPart(config, index, BodyPart.Body);
    }

    internal void CreateTail()
    {
        GameObject tail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Tail"));
        tail.name = "tail";
        tail.transform.parent = robot.transform;
        GameObject lastSection = robotConfig.Configs.Where(o => o.Type == BodyPart.Body && o.Index == robotConfig.NoSections.Value - 1).First().Object;
        tail.transform.localPosition = new Vector3(0, 0, GetZPos(lastSection, tail));

        TailConfig config = tail.GetComponent<TailConfig>();
        if (config == null) config = tail.AddComponent<TailConfig>();
        //setup config - different defaults for tail
        if (!AIConfig.RandomInitValues)
        {
            config.AngleConstraint = new RangedVariable(new Vector3(60, 60, 60), 0, 180, Variable.Physical);
            config.RotationMultiplier = new RangedVariable(new Vector3(1f, 1f, 1f), 0.5f, 1f, Variable.Movement);
        }

        //setup object config
        ObjectConfig objConfig = tail.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = tail.AddComponent<ObjectConfig>();

        //important!!
        objConfig.Init(0, BodyPart.Tail, tail, robotConfig.RobotIndex);
        robotConfig.Configs.Add(objConfig);

        UpdateBodyPart(config, 0, BodyPart.Tail);
    }

    //works for all
    internal void UpdateBodyPart(JointConfig config, int index, BodyPart type)
    {
        GameObject prevSection = null;
        switch (type)
        {
            case BodyPart.Body:
                if (index > 0)
                {
                    prevSection = robotConfig.Configs
                        .Where(o => o.Type == BodyPart.Body && o.Index == index - 1)
                        .First().Object;
                    BodyConfig bodyConfig = (BodyConfig)config;
                    var renderer = config.gameObject.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", new Color(robotConfig.BodyColour.Value / 100f, robotConfig.BodyColour.Value / 100f, 1f));
                    //setup joint
                    SetupConfigurableJoint(config.gameObject, config, prevSection);
                }
                break;
            case BodyPart.Tail:
                prevSection = robotConfig.Configs
                .Where(o => o.Type == BodyPart.Body && o.Index == robotConfig.NoSections.Value - 1)
                .First().Object;
                //set mass to be equal to rest of body
                TailConfig tailConfig = (TailConfig)config;
                config.gameObject.GetComponent<Rigidbody>().mass = GetTotalMass() * tailConfig.TailMassMultiplier.Value;
                //setup joint
                SetupConfigurableJoint(config.gameObject, config, prevSection);
                break;
            case BodyPart.Leg:
                break;
            default:
                break;
        }

    }

    internal void RemoveBody(int index)
    {
        List<ObjectConfig> bodyConfigs = robotConfig.Configs.Where(o => o.Type == BodyPart.Body && o.Index == index).ToList();
        if (bodyConfigs.Count > 1) throw new Exception("There are multiple body parts that fit the criteria for the part being removed after mutation.");
        else if (bodyConfigs.Count == 0) throw new Exception("There are no body parts that fit the criteria for the part being removed after mutation.");
        else
        {
            ObjectConfig bodyConfig = bodyConfigs.First();
            Destroy(bodyConfig.Object);
            robotConfig.Configs.Remove(bodyConfig);
        }

    }
    internal void RemoveTail()
    {
        List<ObjectConfig> tailConfigs = robotConfig.Configs.Where(o => o.Type == BodyPart.Tail).ToList();
        if (tailConfigs.Count > 1) throw new Exception("There are multiple tails that fit the criteria for the part being removed after mutation.");
        else if (tailConfigs.Count == 0) throw new Exception("There are no tails that fit the criteria for the part being removed after mutation.");
        else
        {
            ObjectConfig tailConfig = tailConfigs.First();
            Destroy(tailConfig.Object);
            robotConfig.Configs.Remove(tailConfig);
        }

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

    internal void MakeSerpentine(bool SetRotation = false)
    {
        bool isRotating = true;
        bool useSin = true;
        //go through all body parts and set UseSin (and IsRotating if SetRotation which is used on create)
        foreach (ObjectConfig item in robotConfig.Configs.Where(o => o.Type == BodyPart.Body).ToList())
        {
            GameObject body = item.Object;
            BodyConfig config = body.GetComponent<BodyConfig>();
            if(SetRotation)
            {
                config.IsRotating.Value = isRotating;
                isRotating = !isRotating;
            }
            if (config.IsRotating.Value)
            {
                config.UseSin.Value = useSin;
                useSin = !useSin;
            }
        }
    }

    internal void SetupCam()
    {
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.transform.parent = robot.transform;
        UpdateCam(cam); 
    }

    internal void UpdateCam(GameObject cam)
    {
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
        else if (robotConfig.IsTailEnabled.Value) tail = Tails.First().Object;
        //last section
        List<ObjectConfig> Backs = Sections.Where(o => o.Index == robotConfig.NoSections.Value - 1).ToList();
        if (Backs.Count != 1) throw new Exception("The last section either does not exist or has multiple sections with the same index.");
        else back = Backs.First().Object;

        //give camera the objects it needs
        camPos.Head = head;
        camPos.Tail = robotConfig.IsTailEnabled.Value ? tail : back;
    }

    //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
    internal float GetYPos()
    {
        return TerrainConfig.GetTerrainHeight() + 1f;
    }

    //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
    internal float GetZPos(GameObject prevObject, GameObject thisObject)
    {
        if (prevObject == null) return 0;
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

    internal void SetChildLayer(int newLayer)
    {
        foreach (Transform child in gameObject.transform)
        {
            GameObject childObject = child.gameObject;
            childObject.layer = newLayer;
            //also update the sphere attached to the head to enable collision detection without impacting the behaviour of the robot
            foreach (Transform grandchild in childObject.transform)
            {
                if (grandchild.GetComponent<Rigidbody>() != null)
                {
                    //this is the sphere we're looking for
                    grandchild.gameObject.layer = newLayer;
                }
            }
        }
        //Debug.Log($"Moving {gameObject.name} from layer {LayerMask.LayerToName(oldLayer)} to layer {LayerMask.LayerToName(newLayer)}");
    }
}
