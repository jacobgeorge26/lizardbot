using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveBody : MonoBehaviour
{
    private Rigidbody body;
    private BodyConfig config;
    private Vector3 direction;

    void Awake()
    {        
        //get the rigidbody for this body section as this is how the rotation/position will be manipulated
        body = GetComponent<Rigidbody>();
        config = this.gameObject.AddComponent<BodyConfig>();
    }

    //uses FixedUpdate as recommended for interaction with Unity's physics system
    //will drive and/or rotate as determined in BodyConfig
    void FixedUpdate()
    {        
        if (config.IsDriving) Drive();
        if (config.IsRotating) Rotate();
    }

    public BodyConfig GetBodyConfig()
    {
        return config;
    }

    //TODO: update for 3D rotation
    //rotate this body section
    private void Rotate()
    {
        Vector3 currentAngle = GetRelativeAngle(); //rounded
        Vector3 angleVelocity = new Vector3();
        Vector3 prevSecAngle = new Vector3();

        List<BodyConfig> RotatingSections = BaseConfig.SectionConfigs.Where(s => s.IsRotating && s.Index < config.Index).ToList();
        for (int i = 0; i < 3; i++)
        {          
            //determine initial velocity
            if (RotatingSections.Count > 0)
            {
                BodyConfig previousSection = RotatingSections.Last();
                MoveBody prevSecMoveBody = BaseConfig.Sections[previousSection.Index].GetComponent<MoveBody>();
                prevSecAngle = prevSecMoveBody.GetRelativeAngle();
                angleVelocity[i] = prevSecMoveBody.GetVelocity()[i];
            }
            //TODO: get diameter instead of assuming it
            if (config.UseSin)
            {
                float test = body.transform.localScale.magnitude;

                angleVelocity[i] += 0.5f * ((float)Math.Sin(prevSecAngle[i]) + (float)Math.Sin(currentAngle[i]));
            }
            else
            {
                angleVelocity[i] += 0.5f * ((float)Math.Cos(prevSecAngle[i]) + (float)Math.Cos(currentAngle[i]));
            }       
            //adjust for rotation multiplier
            angleVelocity[i] *= config.RotationMultiplier[i];
        }

        //convert vector to a quaternion
        Quaternion deltaRotation = Quaternion.Euler(angleVelocity * Time.fixedDeltaTime);
        //apply the vector to the body's space and rotate it
        body.MoveRotation(body.rotation * deltaRotation);
        currentAngle = GetRelativeAngle();
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
        body.MovePosition(body.position + direction * config.DriveVelocity * Time.fixedDeltaTime);
    }


    //return angle relative to body within range -180 -> 180. 
    //rounds to int by default as common use of this method is validation about whether to continue turning. 
    public Vector3 GetRelativeAngle(bool round = true)
    {
        Vector3 angle = new Vector3(0, 0, 0);

        //angle should remain 0 for relativity if not rotating
        angle = config.IsRotating ? this.transform.localRotation.eulerAngles : angle;

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
        Vector3 angle = this.transform.localEulerAngles;

        //if opted to then round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;

        return angle;
    }
}
