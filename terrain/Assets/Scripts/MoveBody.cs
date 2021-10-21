using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveBody : BodyConfig
{
    //To Do: move to a BaseConfig / controller setup for parameters applicable to the whole robot

    //variables that indicate which stage in coiling/uncoiling the joint is currently in
    private bool IsCoiling = true;
    private MoveState state = MoveState.Rotate;
    private Rigidbody body;
    private Vector3 direction;
    bool done = false;


    void Start()
    {        
        body = GetComponent<Rigidbody>();
    }

    /* 
    * The joint goes through stages whereby it is rotating or driving
    */
    void FixedUpdate()
    {
        if (IsDriving)
        {
            Drive();
        }
        if (IsRotating)
        {
            Rotate();
        }
    }

    private void Rotate()
    {
        Vector3 currentAngle = GetRelativeAngle(false, true); //rounded & !absolute
        double absrndAngle = Math.Abs(currentAngle.y); //abs version of currentAngle

        //validation
        //if (IsLocked && absrndAngle != 0 && absrndAngle != 180) Debug.LogError($"Joint {this.name} is locked but has y rotation {currentAngle.y}");
        //if (!IsLocked && absrndAngle > MaxAngle) Debug.LogError($"Joint {this.name} has y rotation {currentAngle.y} while the max angle is {MaxAngle}");

        IsClockwise = IsClockwise ? !(currentAngle.y >= MaxAngle) : currentAngle.y <= MaxAngle * -1;
        Vector3 angleVelocity = IsClockwise ? new Vector3(0, TurnVelocity, 0) : new Vector3(0, TurnVelocity * -1, 0);
        Quaternion deltaRotation = Quaternion.Euler(angleVelocity * Time.fixedDeltaTime);
        body.MoveRotation(body.rotation * deltaRotation);
        currentAngle = GetRelativeAngle(false, true); //rounded & !absolute

        //Debug.Log($"Rotating: {IsClockwise};     {currentAngle.y}");
    }

    private void Drive()
    {
        Vector3 currentAngle = GetRelativeAngle(false, true); //rounded & !absolute
        double absrndAngle = Math.Abs(currentAngle.y); //abs version of currentAngle

        direction = this.transform.forward;
        body.MovePosition(body.position + direction * DriveVelocity * Time.fixedDeltaTime);

        //Debug.Log($"Driving: {this.transform.localPosition.z}");
    }


    //return angle relative to body. Will be zero if locked
    public Vector3 GetRelativeAngle(bool abs = false, bool round = false)
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

        //if opted to then absolute &| round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;
        angle = abs ? new Vector3((float)Math.Abs(angle.x), (float)Math.Abs(angle.y), (float)Math.Abs(angle.z)) : angle;

        return angle;
    }

    //return actual angles - not relative to body
    public Vector3 GetAngle(bool abs = false, bool round = false)
    {
        Vector3 angle = this.transform.localEulerAngles;

        //if opted to then absolute &| round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;
        angle = abs ? new Vector3((float)Math.Abs(angle.x), (float)Math.Abs(angle.y), (float)Math.Abs(angle.z)) : angle;

        return angle;
    }


    private enum MoveState
    {
        Rotate = 0,
        Drive = 1,
        Complete = 2
    }

    private enum LockOption
    {
        Forward = 0,
        Backward = 1
    }
}
