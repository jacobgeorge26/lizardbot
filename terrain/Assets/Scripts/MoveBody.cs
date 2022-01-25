using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveBody : MonoBehaviour
{
    private RobotConfig robotConfig;
    private Rigidbody body;
    private BodyConfig config;
    private ObjectConfig objectConfig;  
    private Vector3 direction;
    private bool IsEnabled = false;

    private List<ObjectConfig> RotatingConfigs = new List<ObjectConfig>();

    void Start()
    {
        //get the rigidbody for this body section as this is how the rotation/position will be manipulated
        body = GetComponent<Rigidbody>();
        config = this.gameObject.GetComponent<BodyConfig>();
        objectConfig = this.gameObject.GetComponent<ObjectConfig>();
        robotConfig = AIConfig.RobotConfigs.Where(c => c.RobotIndex.Equals(objectConfig.RobotIndex)).First();
    }


    private void SetupRotatingSections()
    {
        foreach (ObjectConfig objConfig in robotConfig.Configs.Where(o => o.Type == BodyPart.Body))
        {
            GameObject obj = objConfig.Object;
            BodyConfig bodyConfig = obj.GetComponent<BodyConfig>();
            if (bodyConfig.IsRotating.Value)
            {
                RotatingConfigs.Add(objConfig);
            }
        }
    }

    //public for tests to call - limited setup
    public void EditModeTestInit()
    {
        body = GetComponent<Rigidbody>();
    }


    //uses FixedUpdate as recommended for interaction with Unity's physics system
    //will drive and/or rotate as determined in BodyConfig
    void FixedUpdate()
    {        
        if (config.IsDriving.Value && IsEnabled) Drive();
        if (config.IsRotating.Value && IsEnabled) Rotate();
    }

    //rotate this body section
    private void Rotate()
    {
        Vector3 currentAngle = GetRelativeAngle(false);
        Vector3 angleVelocity = new Vector3();
        Vector3 prevSecAngle = new Vector3();
        //determine initial velocity
        List<ObjectConfig> prevRotating = RotatingConfigs.Where(o => o.Index < objectConfig.Index).ToList();
        if(prevRotating.Count > 0)
        {
            GameObject previousSection = prevRotating.Last().Object;
            MoveBody prevSecMoveBody = previousSection.GetComponent<MoveBody>();
            prevSecAngle = prevSecMoveBody.GetRelativeAngle();
            angleVelocity = prevSecMoveBody.GetVelocity();
        }

        for (int i = 0; i < 3; i++)
        {          
            //sin & cos use radians, convert angles from degrees to radians for these
            float radiansPrevAngle = (float)(Math.PI / 180f * prevSecAngle[i]);
            float radiansCurrAngle = (float)(Math.PI / 180f * currentAngle[i]);
            //use CPG equation to get the velocity that each axis should have
            if (config.UseSin.Value)
            {
                angleVelocity[i] += body.transform.localScale[i] * 0.5f * ((float)Math.Sin(radiansPrevAngle) + (float)Math.Sin(radiansCurrAngle));
            }
            else
            {
                angleVelocity[i] += body.transform.localScale[i] * 0.5f * ((float)Math.Cos(radiansPrevAngle) + (float)Math.Cos(radiansCurrAngle));
            }       
            //adjust for rotation multiplier
            angleVelocity[i] *= config.RotationMultiplier.Value[i];
        }
        //apply force
        //relative force uses local space vs world space
        //force mode is set to velocity change as this is what the equation is looking at
        //admittedly, each of these details has negligible differences in behaviour vs AddForce and ForceMode.Force
        //0.1 is used to bring the force to a level that it won't cause it to self-destruct, the RotationMultiplier can be used to adjust the force applied in each axis if desired
        body.AddRelativeForce(angleVelocity * 0.1f, ForceMode.VelocityChange);
    }

    private Vector3 GetVelocity()
    {
        return body.velocity;
    }

    //drive this body section forward
    private void Drive()
    {
        //get the current trajectory of the body section
        direction = this.transform.forward;
        //move it forward at a speed derived in BodyConfig
        body.MovePosition(body.position + direction * config.DriveVelocity.Value * Time.fixedDeltaTime);
    }


    //return angle relative to body within range -180 -> 180. 
    //rounds to int by default as common use of this method is validation about whether to continue turning. 
    public Vector3 GetRelativeAngle(bool round = true)
    {
        Vector3 angle = body.rotation.eulerAngles;

        //update for range -180 - 180
        angle.x -= Math.Round(angle.x, 0) > 180 ? 360 : 0;
        angle.y -= Math.Round(angle.y, 0) > 180 ? 360 : 0;
        angle.z -= Math.Round(angle.z, 0) > 180 ? 360 : 0;

        angle.x += Math.Round(angle.x, 0) < -180 ? 360 : 0;
        angle.y += Math.Round(angle.y, 0) < -180 ? 360 : 0;
        angle.z += Math.Round(angle.z, 0) < -180 ? 360 : 0;

        //if opted to then round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;

        return angle;
    }

    //return actual angles - not relative to body
    //has option to get rounded to int but won't by default
    public Vector3 GetAngle(bool round = false)
    {
        Vector3 angle = body.rotation.eulerAngles;

        //if opted to then round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;

        return angle;
    }

    public void Enable()
    {
        SetupRotatingSections();
        IsEnabled = true;
    }

}
