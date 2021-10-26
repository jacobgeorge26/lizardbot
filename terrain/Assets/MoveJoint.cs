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
                if (IsCoiling) StartCoroutine(ToggleJointLock(true, LockOption.Backward));
                else StartCoroutine(ToggleJointLock(true, LockOption.Forward));
                state = MoveState.Rotate;
                break;

            case MoveState.Rotate:
                Coil(); //state is updated in coil / uncoil
                break;
            case MoveState.Unlock:
                if (IsCoiling) StartCoroutine(ToggleJointLock(false, LockOption.Backward));
                else StartCoroutine(ToggleJointLock(false, LockOption.Forward));
                state = MoveState.Recalibrate;
                break;
            case MoveState.Recalibrate:
                RecalibrateJoint();
                IsClockwise = IsCoiling ? !IsClockwise : IsClockwise; //change direction next time
                state = MoveState.Lock;
                IsCoiling = !IsCoiling;
                break;
        }
    }

    private void RecalibrateJoint()
    {
        double recalAngle = 0;
        Vector3 currentAngle = GetRelativeAngle();

        double rndAngle = Math.Round(currentAngle.y); //rnd version of currentAngle.y

        //validation
        //there's been a bug and this joint is rotated when it shouldn't be
        if (IsLocked && rndAngle > 0) Debug.LogWarning($"Joint {Joint.name} was rotated when it is locked");
        //there's been a bug and this joint isn't rotated as it should be
        if (IsCoiling && !IsLocked && Math.Abs(rndAngle) - Math.Abs(MaxAngle) > 1) Debug.LogWarning($"Joint {Joint.name} has y rotation {rndAngle} after coiling when max angle is {MaxAngle}");

        //return the angle to relative angle 0
        //this will correspond to the angle * -1 when it locks the other way in a moment
        recalAngle = currentAngle.y * -1;

        //if locked then flip it round so that it faces the other section
        //helps with debugging
        recalAngle += IsLocked ? 180 : 0;

        Joint.transform.Rotate(0, (float)recalAngle, 0);
    }

    private void Coil()
    {
        bool stillRotating = false;
        Vector3 currentAngle = GetRelativeAngle(false, true); //rounded & !absolute
        double absrndAngle = Math.Abs(currentAngle.y); //abs version of currentAngle

        //validation
        if (IsLocked && absrndAngle != 0 && absrndAngle != 180) Debug.LogError($"Joint {Joint.name} is locked but has y rotation {currentAngle.y}");
        if (!IsLocked && absrndAngle > MaxAngle) Debug.LogError($"Joint {Joint.name} has y rotation {currentAngle.y} while the max angle is {MaxAngle}");

        if (!IsLocked)
        {
            if (IsCoiling && absrndAngle < MaxAngle) //joint needs coiling
            {
                stillRotating = true;
                double turnAngle = IsClockwise ? MaxAngle : MaxAngle * -1; //coiling clockwise or anticlockwise
                StartCoroutine(RotateJoint(turnAngle));
            }
            else if (!IsCoiling && absrndAngle > 0) //joint needs uncoiling
            {
                stillRotating = true;
                double turnAngle = 0;
                StartCoroutine(RotateJoint(turnAngle));
            }
        }
        state = stillRotating ? MoveState.Rotate : MoveState.Unlock;        
    }

    //return angle relative to body. Will be zero if locked
    public Vector3 GetRelativeAngle(bool abs = false, bool round = false)
    {
        Vector3 angle = new Vector3(0, 0, 0);

        //angle should remain 0 for relativity if locked
        angle = IsLocked ? angle : Joint.transform.localEulerAngles;

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
        Vector3 angle = Joint.transform.localEulerAngles;

        //if opted to then absolute &| round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;
        angle = abs ? new Vector3((float)Math.Abs(angle.x), (float)Math.Abs(angle.y), (float)Math.Abs(angle.z)) : angle;

        return angle;
    }

    //use lerp to rotate joint to target angle 
    //NOTE - uses relative angles!
    private IEnumerator RotateJoint(double targetAngle)
    {
        float moveSpeed = 0.005f;
        Vector3 currentAngle = GetRelativeAngle();
        while ((targetAngle > 0 && currentAngle.y < targetAngle) || (targetAngle < 0 && currentAngle.y > targetAngle) || (targetAngle == 0 && currentAngle.y != 0))
        {
            Joint.transform.localRotation = Quaternion.Lerp(Joint.transform.localRotation, Quaternion.Euler(0, (float)targetAngle, 0), moveSpeed);
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
}
