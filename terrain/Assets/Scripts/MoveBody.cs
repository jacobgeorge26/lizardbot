using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveBody : BodyConfig
{
    //TODO: move to a BaseConfig / controller setup for parameters applicable to the whole robot

    private Rigidbody body;
    private Vector3 direction;


    void Start()
    {        
        //get the rigidbody for this body section as this is how the rotation/position will be manipulated
        body = GetComponent<Rigidbody>();
    }

    //uses FixedUpdate as recommended for interaction with Unity's physics system
    //will drive and/or rotate as determined in BodyConfig
    void FixedUpdate()
    {
        if (IsDriving) Drive();
        if (IsRotating) Rotate();
    }

    //TODO: update for 3D rotation
    //rotate this body section
    private void Rotate()
    {
        Vector3 currentAngle = GetRelativeAngle(); //rounded
        //if it's reached the max angle (pos or neg) then reverse direction
        IsClockwise = IsClockwise ? !(currentAngle.y >= MaxAngle) : currentAngle.y <= MaxAngle * -1; 
        //determine its velocity vector, TurnVelocity is deg/sec and is derived in BodyConfig
        Vector3 angleVelocity = IsClockwise ? new Vector3(0, TurnVelocity, 0) : new Vector3(0, TurnVelocity * -1, 0);
        //convert vector to a quaternion
        Quaternion deltaRotation = Quaternion.Euler(angleVelocity * Time.fixedDeltaTime);
        //apply the vector to the body's space and rotate it
        body.MoveRotation(body.rotation * deltaRotation);
    }

    //drive this body section forward
    private void Drive()
    {
        //get the current trajectory of the body section
        direction = this.transform.forward;
        //move it forward at a speed derived in BodyConfig
        body.MovePosition(body.position + direction * DriveVelocity * Time.fixedDeltaTime);
    }


    //return angle relative to body within range -180 -> 180. 
    //rounds to int by default as common use of this method is validation about whether to continue turning. 
    //If within a degree of maxangle should reverse direction
    public Vector3 GetRelativeAngle(bool round = true)
    {
        Vector3 angle = new Vector3(0, 0, 0);

        //angle should remain 0 for relativity if locked
        angle = IsRotating ? this.transform.localRotation.eulerAngles : angle;

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
