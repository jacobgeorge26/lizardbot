using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveJoint : JointConfig
{
    //Parameters
    [SerializeField]
    private bool stepUncoil;

    //testing variables
    private bool IsCoiling = true;
    private MoveState state = MoveState.Lock;

    void Start()
    {        
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case MoveState.Lock:
                if (IsCoiling)
                {
                    StartCoroutine(ToggleJointLock(true, LockOption.Backward));
                }
                else
                {
                   StartCoroutine(ToggleJointLock(true, LockOption.Forward));
                }
                StoreAngle();
                state = MoveState.Rotate;
                break;
            case MoveState.Rotate:
                Coil();
                //state is updated in coil / uncoil
                break;
            case MoveState.Unlock:
                StoreAngle();

                break;
            case MoveState.Recalibrate:
                break;
        }
    }

    private void Coil()
    {
        bool stillRotating = false;
        double absrnd_y = AbsRnd(RelativeAngle.y);

        //validation
        if (IsLocked && absrnd_y != 0 && absrnd_y != 180) Debug.LogError($"Joint {Joint.name} is locked but has y rotation {Rnd(RelativeAngle.y)}");
        if (!IsLocked && absrnd_y > MaxAngle) Debug.LogError($"Joint {Joint.name} has y rotation {Rnd(RelativeAngle.y)} while the max angle is {MaxAngle}");

        if (!IsLocked)
        {
            if (IsCoiling && absrnd_y < MaxAngle) //joint needs coiling
            {
                stillRotating = true;
                double turnAngle = IsClockwise ? MaxAngle : MaxAngle * -1; //coiling clockwise or anticlockwise
                StartCoroutine(RotateJoint(turnAngle));
            }
            else if (!IsCoiling && absrnd_y > 0) //joint needs uncoiling
            {
                stillRotating = true;
                double turnAngle = 0;
                StartCoroutine(RotateJoint(turnAngle));
            }
        }
        state = stillRotating ? MoveState.Rotate : MoveState.Unlock;        
    }

    //angle = final angle
    private IEnumerator RotateJoint(double angle)
    {
        float moveSpeed = 0.0005f;
        while ((angle > 0 && Joint.transform.localEulerAngles.y < angle) || (angle < 0 && Joint.transform.localEulerAngles.y > angle) || (angle == 0 && Joint.transform.localEulerAngles.y != 0))
        {
            Joint.transform.localRotation = Quaternion.Lerp(Joint.transform.localRotation, Quaternion.Euler(0, (float)angle, 0), moveSpeed);
            yield return null;
            if (state != MoveState.Rotate) yield break;
        }
    }

    private IEnumerator ToggleJointLock(bool lockBody, LockOption direction)
    {
        if (direction == LockOption.Backward)
        {
            Joint.transform.parent = PreviousSection.transform;
            NextSection.transform.parent = lockBody ? Joint.transform : null; 
        }
        else if (direction == LockOption.Forward)
        {
            Joint.transform.parent = lockBody ? NextSection.transform : PreviousSection.transform;
            PreviousSection.transform.parent = lockBody ? Joint.transform : null;
        }
        yield return null;
    }

    private enum MoveState
    {
        Lock = 0,
        Rotate = 1,
        Unlock = 2,
        Recalibrate = 3
    }

    private enum LockOption
    {
        Forward = 0,
        Backward = 1
    }

    private double AbsRnd(float angle)
    {
        return Math.Abs(Math.Round(angle, 0));
    }

    private double Rnd(float angle)
    {
        return Math.Round(angle, 0);
    }

    private void StoreAngle()
    {
        RelativeAngle = new Vector3();
        if (!IsLocked) //this joint is not locked and will be rotating
        {
            RelativeAngle = Joint.transform.localEulerAngles;
            RelativeAngle.y -= Rnd(RelativeAngle.y) > 180 ? 360 : 0;
            RelativeAngle.y += Rnd(RelativeAngle.y) < -180 ? 360 : 0;

        }
    }
}
