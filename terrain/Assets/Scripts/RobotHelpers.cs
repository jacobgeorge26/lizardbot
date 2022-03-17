using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class RobotHelpers : object
{
    //create the head - has less to it than setting up a main body section
    //should only be called at start as the head should never be removed - NoSections > 0
    internal static void CreateHead(this RobotConfig robot, ObjectConfig existingBody = null)
    {
        GameObject head = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Head"));
        head.name = "head";
        head.transform.parent = robot.Object.transform;
        head.transform.localPosition = new Vector3(0, 0, robot.GetZPos(null, head));

        //setup BodyConfig for MoveBody script
        BodyConfig config = new BodyConfig();
        if (existingBody != null) config.Clone(existingBody.Body);

        ObjectConfig objConfig = head.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = head.AddComponent<ObjectConfig>();
        //important!!
        objConfig.Init(0, BodyPart.Body, config, robot.RobotIndex);
        robot.Configs.Add(objConfig);

        robot.UpdateBodyPart(objConfig, 0, BodyPart.Body);
    }

    //create a body section and attach it to the previous section
    internal static void CreateBody(this RobotConfig robot, int index, ObjectConfig existingBody = null)
    {
        GameObject body = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        GameObject prevSection = null;
        try { prevSection = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == index - 1).gameObject; }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
        body.name = $"section{index}";
        body.transform.parent = robot.Object.transform;
        body.transform.localPosition = new Vector3(0, 0, robot.GetZPos(prevSection, body));

        //setup BodyConfig for MoveBody script
        BodyConfig config = new BodyConfig();
        if (existingBody != null) config.Clone(existingBody.Body);

        ObjectConfig objConfig = body.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = body.AddComponent<ObjectConfig>();
        //important!!
        objConfig.Init(index, BodyPart.Body, config, robot.RobotIndex);
        robot.Configs.Add(objConfig);

        robot.UpdateBodyPart(objConfig, index, BodyPart.Body);
    }

    //create a tail - only ever one
    internal static void CreateTail(this RobotConfig robot, ObjectConfig existingTail = null)
    {
        GameObject tail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Tail"));
        tail.name = "tail";
        tail.transform.parent = robot.Object.transform;
        GameObject lastSection = null;
        try { lastSection = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == robot.NoSections.Value - 1).gameObject; }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
        
        tail.transform.localPosition = new Vector3(0, 0, robot.GetZPos(lastSection, tail));

        TailConfig config = new TailConfig();
        if (existingTail != null) config.Clone(existingTail.Tail);

        //setup config - different defaults for tail
        if (!AIConfig.RandomInitValues)
        {
            config.AngleConstraint = new Gene(new Vector3(60, 60, 60), 0, 180, Variable.AngleConstraint);
            config.RotationMultiplier = new Gene(new Vector3(1f, 1f, 1f), 0.5f, 1f, Variable.RotationMultiplier);
        }

        //setup object config
        ObjectConfig objConfig = tail.GetComponent<ObjectConfig>();
        if (objConfig == null) objConfig = tail.AddComponent<ObjectConfig>();

        //important!!
        objConfig.Init(0, BodyPart.Tail, config, robot.RobotIndex);
        robot.Configs.Add(objConfig);

        robot.UpdateBodyPart(objConfig, 0, BodyPart.Tail);
    }

    //works for all body types (body, tail etc.) - updates it for any changes to its associated config
    //must be called after creation too, to initialise it with the config params
    internal static void UpdateBodyPart(this RobotConfig robot, ObjectConfig objConfig, int index, BodyPart type)
    {
        GameObject prevSection = null;
        switch (type)
        {
            case BodyPart.Body:
                if (index > 0)
                {
                    try {
                        prevSection = robot.Configs
                            .Where(o => o.Type == BodyPart.Body && o.Index == index - 1)
                            .First().gameObject;
                    }
                    catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
 
                    BodyConfig bodyConfig = objConfig.Body;
                    var renderer = objConfig.gameObject.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", new Color(robot.BodyColour.Value / 100f, robot.BodyColour.Value / 100f, 1f));
                    //setup joint
                    robot.SetupConfigurableJoint(objConfig.gameObject, bodyConfig, prevSection);
                }
                break;
            case BodyPart.Tail:
                try {
                    prevSection = robot.Configs
                        .Where(o => o.Type == BodyPart.Body && o.Index == robot.NoSections.Value - 1)
                        .First().gameObject;
                }
                catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
                //set mass to be equal to rest of body
                TailConfig tailConfig = objConfig.Tail;
                objConfig.gameObject.GetComponent<Rigidbody>().mass = robot.GetTotalMass() * tailConfig.TailMassMultiplier.Value;
                //setup joint
                robot.SetupConfigurableJoint(objConfig.gameObject, tailConfig, prevSection);
                //reset position in case a section has been removed
                objConfig.gameObject.transform.localPosition = new Vector3(0, 0, robot.GetZPos(prevSection, objConfig.gameObject));
                break;
            case BodyPart.Leg:
                break;
            default:
                break;
        }

    }

    //remove a body section
    internal static void RemoveBody(this RobotConfig robot, int index)
    {
        try {
            ObjectConfig bodyConfig = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == index);
            bodyConfig.Remove();
            robot.Configs.Remove(bodyConfig);
        }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
    }

    //remove the tail
    internal static void RemoveTail(this RobotConfig robot)
    {
        List<ObjectConfig> tailConfigs = robot.Configs.Where(o => o.Type == BodyPart.Tail).ToList();
        if (tailConfigs.Count > 1) throw new Exception("There are multiple tails that fit the criteria for the part being removed after mutation.");
        else if (tailConfigs.Count == 0) throw new Exception("There are no tails that fit the criteria for the part being removed after mutation.");
        else
        {
            ObjectConfig tailConfig = null;
            try { tailConfig = tailConfigs.First(); }
            catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
            tailConfig.Remove();
            robot.Configs.Remove(tailConfig);
        }
    }

    //setup the joints to have the correct angle constraints and be attached to the correct rigidbody before it in the body
    private static void SetupConfigurableJoint(this RobotConfig robot, GameObject section, JointConfig config, GameObject prevObject)
    {
        ConfigurableJoint joint = section.GetComponent<ConfigurableJoint>();
        joint.lowAngularXLimit = new SoftJointLimit() { limit = -1 * config.AngleConstraint.Value[0] / 2 };
        joint.highAngularXLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[0] / 2 };
        joint.angularYLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[1] / 2 };
        joint.angularZLimit = new SoftJointLimit() { limit = config.AngleConstraint.Value[2] / 2 };
        joint.connectedBody = prevObject.GetComponent<Rigidbody>();
    }

    //used when creating a new body section for an existing robot
    //set it as an average of the existing body, so that it is 'up to date' with the evolution thus far
    internal static void AverageRestOfBody(this RobotConfig robot)
    {
        //get all the genes for the rest of the body
        List<ObjectConfig> prevBody = robot.Configs.Where(o => o.Type == BodyPart.Body).OrderBy(o => o.Index).ToList();
        ObjectConfig last = prevBody.Last();
        prevBody.Remove(last);
        List<Gene> allGenes = new List<Gene>();
        foreach (var objConfig in prevBody)
        {
            allGenes = allGenes.Concat(robot.GetVariables(objConfig.Body)).ToList();
        }
        //get all the genes for the last body section
        List<Gene> newGenes = new List<Gene>();
        BodyConfig newBody = last.Body;
        newGenes = newGenes.Concat(robot.GetVariables(newBody)).ToList();
        //now there's a list of all the genes that need to be updated (newGenes) and all the genes of the rest of the body (allGenes)
        foreach (var item in newGenes)
        {
            int count = 0;
            if (item.Real.GetType() == typeof(Vector3))
            {
                Vector3 sum = Vector3.zero;
                allGenes.Where(o => o.Type == item.Type).ToList().ForEach(o => { sum += o.Value; count++; });
                item.Value = sum / count;
            }
            else
            {
                dynamic sum = item.Real;
                sum = 0;
                allGenes.Where(o => o.Type == item.Type).ToList().ForEach(o => { sum += o.Real; count++; });
                item.Value = Convert.ChangeType(sum / count, item.Real.GetType());
            }
        }
        //if there is fixed alternating rotating sections, then set this up. The call for serpentine will be make in the GA
        if (!AIConfig.RandomInitValues && prevBody.Count > 0)
        {
            BodyConfig prevBodyConfig = prevBody.Last().Body;
            newBody.IsRotating.Value = !prevBodyConfig.IsRotating.Value;
        }
    }

    //set the rotating sections to alternate between sin and cos
    //if SetRotation, then also set it such that every other section is rotating
    //SetRotation should only really be true when a robot is first created
    internal static void MakeSerpentine(this RobotConfig robot, bool SetRotation = false)
    {
        bool isRotating = true;
        bool useSin = true;
        //go through all body parts and set UseSin (and IsRotating if SetRotation which is used on create)
        foreach (ObjectConfig item in robot.Configs.Where(o => o.Type == BodyPart.Body).ToList())
        {
            GameObject body = item.gameObject;
            BodyConfig config = item.Body;
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

    //get a list of all the genevariables for a config (e.g. BodyConfig)
    internal static List<Gene> GetVariables(this RobotConfig robot, dynamic config)
    {
        List<Gene> genes = new List<Gene>();
        if (config == null) return genes;
        var allFields = config.GetType().GetFields();
        foreach (var item in allFields)
        {
            if (item.FieldType == typeof(Gene))
            {
                var f = item.GetValue(config);
                genes.Add((Gene)f);
            }
        }
        return genes;
    }

    //attach the cam to the robot, say after switching to a new robot in the Robot UI View
    internal static void AttachCam(this RobotConfig robot)
    {
        //if the camera was previously attached to another robot then remove that script
        if(CameraConfig.CamFollow == -1)
        {
            CameraConfig.RobotCamera.SetActive(true);
        }
        CameraConfig.CamFollow = robot.RobotIndex;
        CameraConfig.RobotCamera.GetComponent<CameraPosition>().SetRobot(robot);
    }

    //get what the Y position param should be on spawn
    //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
    internal static float GetYPos(this RobotConfig robot)
    {
        return TerrainConfig.GetTerrainHeight(Mathf.FloorToInt(robot.RobotIndex / 25)) + 1f;
    }

    //get what the Z position param should be on spawn - called for each body part
    //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
    internal static float GetZPos(this RobotConfig robot, GameObject prevObject, GameObject thisObject)
    {
        if (prevObject == null) return 0;
        //determine the position of this section by the location of the prev section, and size of both
        float zPos = prevObject.transform.localPosition.z;
        zPos += -1 * (prevObject.transform.localScale.z / 2 + thisObject.transform.localScale.z / 2 + 0.1f);
        return zPos;
    }

    //return the total mass of the robot
    private static float GetTotalMass(this RobotConfig robot)
    {
        float sumMass = 0;
        //iterate objects, get sum of (mass * axis coordinate), divide by sum of all masses
        foreach (ObjectConfig objConfig in robot.Configs)
        {
            sumMass += objConfig.gameObject.GetComponent<Rigidbody>().mass;
        }
        return sumMass;
    }

    //when changing the layer of a robot, update each child to also use this layer
    internal static void SetChildLayer(this RobotConfig robot, int newLayer)
    {
        foreach (Transform child in robot.Object.transform)
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

    //get all robots within a radius
    internal static List<RobotConfig> GetNearbyRobots(this RobotConfig robot, int radius)
    {
        Vector3 thisInitPos = AIConfig.SpawnPoints[Mathf.FloorToInt(robot.RobotIndex / 25)], thisRelPos = Vector3.zero;
        try { thisRelPos = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == 0).gameObject.transform.position - thisInitPos; }
        catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
        List<RobotConfig> nearby = new List<RobotConfig>();
        AIConfig.RobotConfigs.ForEach(r =>
        {
            if (r.IsEnabled)
            {
                GameObject head = null;
                try { head = r.Configs.First(o => o.Type == BodyPart.Body && o.Index == 0).gameObject; }
                catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
                       
                Vector3 initPos = AIConfig.SpawnPoints[Mathf.FloorToInt(r.RobotIndex / 25)];
                Vector3 relativePos = head.transform.position - initPos;
                if (Vector3.Distance(relativePos, thisRelPos) <= radius)
                {
                    nearby.Add(r);
                }
            }
        });
        return nearby;
    }

    //get all robots that are physically similar, within a set accepted range of difference
    internal static List<RobotConfig> GetPhysicallySimilarRobots(this RobotConfig robot, int difference)
    {
        List<RobotConfig> similar = new List<RobotConfig>();
        //higher the difference, the more different the robot is allowed to be
        //find all robots with the same number of sections, and a tail (or lack thereof)
        similar = AIConfig.RobotConfigs.Where(r => 
            r.RobotIndex != robot.RobotIndex && 
            Math.Abs(r.NoSections.Value - robot.NoSections.Value) <= difference &&
            r.IsTailEnabled.Value == robot.IsTailEnabled.Value &&
            r.IsEnabled).ToList();
        //remove those whose tail is very different
        //allowed difference = (max - min) / 10 * difference
        if (robot.IsTailEnabled.Value)
        {
            if(robot.Configs.Where(o => o.Type == BodyPart.Tail).ToList().Count == 0)
            {
                Debug.LogWarning($"The tail is missing for robot {robot.RobotIndex + 1} whilst getting robots physically similar to it. The tail is being created before the recombination continues");
                robot.CreateTail();
            }
            TailConfig tail = null;
            try { tail = robot.Configs.First(o => o.Type == BodyPart.Tail).Tail; }
            catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
            
            float allowedDifference = (tail.TailMassMultiplier.Max - tail.TailMassMultiplier.Min) * (difference + 1) / 10;
            for (int i = similar.Count - 1; i >= 0; i--)
            {
                List<ObjectConfig> tails = similar[i].Configs.Where(o => o.Type == BodyPart.Tail).ToList();
                if(tails.Count != 1)
                {
                    Debug.LogWarning($"The tail is missing for robot {similar[i].RobotIndex + 1} whilst getting physically similar robots. The tail is being created and this robot will not be considered for recombination.");
                    similar[i].CreateTail();
                    similar.RemoveAt(i);
                    continue;
                }
                TailConfig otherTail = null;
                try { otherTail = tails.First().Tail; }
                catch (Exception ex) { GameController.Controller.Respawn(ex.ToString()); }
                
                if (Math.Abs(otherTail.TailMassMultiplier.Value - tail.TailMassMultiplier.Value) > allowedDifference)
                {
                    similar.RemoveAt(i);
                }
            }
        }
        return similar;
    }

    //get all robots that are movement-ally similar, within a set accepted range of difference
    internal static List<RobotConfig> GetMovementSimilarRobots(this RobotConfig robot, int difference)
    {
        List<RobotConfig> similar = new List<RobotConfig>();
        //higher the difference, the more different the robot is allowed to be
        //find all robots that are also preserving serpentine motion
        similar = AIConfig.RobotConfigs.Where(r =>
            r.RobotIndex != robot.RobotIndex &&
            r.MaintainSerpentine.Value == robot.MaintainSerpentine.Value &&
            r.IsEnabled).ToList();
        float thisDriveVelocity = 0;
        if(similar.Count > 0)
        {
            //get the total drive velocity and rotation multiplier for this robot
            List<ObjectConfig> bodyObjects = robot.Configs.Where(o => o.Type == BodyPart.Body).ToList();
            List<BodyConfig> body = new List<BodyConfig>();
            bodyObjects.ForEach(o => body.Add(o.Body));
            body.ForEach(o => {
                thisDriveVelocity += o.IsDriving.Value ? o.DriveVelocity.Value : 0;
            });
        }
        float allowedDifference = 0.25f * (difference + 1);
        //remove those with a different similarity
        for (int i = similar.Count - 1; i >= 0; i--)
        {
            //how different is the drive velocity (zero if not driving, to take into account no of driving sections)
            //how different are the rotation multipliers
            List<ObjectConfig> bodyObjects = similar[i].Configs.Where(o => o.Type == BodyPart.Body).ToList();
            List<BodyConfig> body = new List<BodyConfig>();
            bodyObjects.ForEach(o => body.Add(o.Body));
            //get the total for the other robot
            float otherDriveVelocity = 0;
            body.ForEach(o => {
                otherDriveVelocity += o.IsDriving.Value ? o.DriveVelocity.Value : 0;
            });
            //remove if the drive velocity : nosections is too different
            if (Math.Abs((thisDriveVelocity / robot.NoSections.Value) - (otherDriveVelocity / similar[i].NoSections.Value)) > allowedDifference) similar.RemoveAt(i);
        }
        return similar;
    }

    //get the expected number of colliders that would be within the range of the robot anyway (such as the terrain and the size of the robot itself)
    private static int GetExpectedColliders(this RobotConfig robot)
    {
        int expectedColliders = 0;
        expectedColliders = robot.NoSections.Value; //one for each section of the body
        expectedColliders++; //terrain
        expectedColliders++; //sphere contained in head for collision detection
        expectedColliders += robot.IsTailEnabled.Value ? 1 : 0; //tail
        return expectedColliders;
    }
}
