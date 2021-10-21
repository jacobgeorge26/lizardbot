using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;
using Helpers;

public class Move : BodyHelpers
{
    //Parameters
    [SerializeField]
    private bool stepUncoil;

    //Calculated
    private Vector3[] relativeAngles;

    //testing variables
    bool coiling = true;
    MoveState state = MoveState.Lock;

    private void Start()
    {
        relativeAngles = new Vector3[Joints.Count];
    }

    //To Do: when JointConfig is working use queue and limit to unlocked joints only
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    void Update()
    {
        switch (state)
        {
            case MoveState.Lock:
                if (coiling)
                {
                    StartCoroutine(ToggleBodyLock(0, true, LockOption.Backward));
                }
                else
                {
                    StartCoroutine(ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward));
                }
                state = MoveState.Rotate;
                break;

            case MoveState.Rotate:
                Coil();
                //state is updated in coil / uncoil
                break;

            case MoveState.Unlock:
                StoreAngles();
                if (coiling)
                {
                    StartCoroutine(ToggleBodyLock(0, false, LockOption.Backward));
                }
                else
                {
                    StartCoroutine(ToggleBodyLock(Joints.Count - 1, false, LockOption.Forward));
                }
                state = MoveState.Recalibrate;
                break;

            case MoveState.Recalibrate:
                RecalibrateJoints();
                if (coiling) ToggleDirection();                

                state = MoveState.Lock;
                coiling = !coiling;
                break;
        }
    }

    private void ToggleDirection()
    {
        for (int i = 0; i < Direction.Length; i++)
        {
            Direction[i] = !Direction[i];
        }
    }

    private void Coil()
    {
        bool stillRotating = false;
        for (int i = 0; i < Joints.Count; i++)
        {
            int index = coiling ? i : Joints.Count - 1 - i; //uncoiling going back to front

            GameObject joint = Joints[index];
            Vector3 currentAngle = GetAngle(joint);
            double absrnd_y = AbsRnd(currentAngle.y);

            //validation
            if (Locked[index] && absrnd_y != 0 && absrnd_y != 180) Debug.LogError($"Joint {index + 1} is locked but has y rotation {Math.Round(currentAngle.y, 2)}");
            if (!Locked[index] && absrnd_y > maxAngle) Debug.LogError($"Joint {index + 1} has y rotation {Math.Round(currentAngle.y, 2)} while the max angle is {maxAngle}");


            if (!Locked[index])
            {
                if (coiling && absrnd_y < maxAngle) //joint needs coiling
                {
                    stillRotating = true;

                    double turnAngle = Direction[index] ? maxAngle : maxAngle * -1; //coiling clockwise or anticlockwise

                    StartCoroutine(RotateJoint(joint, turnAngle));
                }
                else if (!coiling && absrnd_y > 0) //joint needs uncoiling
                {
                    //if stepUncoil is true then it will uncoil one joint at a time (tail to head) 
                    if (!stepUncoil || (stepUncoil && !stillRotating))
                    {
                        stillRotating = true;

                        double turnAngle = 0; //different

                        StartCoroutine(RotateJoint(joint, turnAngle));
                    }
                }

            }
        }
        state = stillRotating ? MoveState.Rotate : MoveState.Unlock;
    }

    private void StoreAngles()
    {
        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];
            relativeAngles[index] = Locked[index] ? new Vector3() : GetAngle(joint);
        }
        printRelativeAngles(relativeAngles);
    }


    private void RecalibrateJoints()
    {
        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];
            double recalAngle = 0;
            //validation
            //there's been a bug and this joint is rotated when it shouldn't be
            if (Locked[index] && Math.Round(relativeAngles[index].y, 0) > 0) Debug.LogWarning($"Joint {index + 1} was rotated when it is locked");
            //there's been a bug and this joint isn't rotated as it should be
            if (coiling && !Locked[index] && AbsRnd(relativeAngles[index].y) != maxAngle) Debug.LogWarning($"Joint {index + 1} has y rotation {Math.Round(relativeAngles[index].y, 0)} after coiling when max angle is {maxAngle}");

            recalAngle = relativeAngles[index].y * -1;
            if (Locked[index])
            {
                recalAngle += 180;
            }

            joint.transform.Rotate(0, (float)recalAngle, 0);
        }
    }


    //angle = final angle
    private IEnumerator RotateJoint(GameObject joint, double angle)
    {
        float moveSpeed = 0.00005f;
        Vector3 currentAngle = GetAngle(joint);
        while ((angle > 0 && currentAngle.y < angle) || (angle < 0 && currentAngle.y > angle) || (angle == 0 && currentAngle.y != 0))
        {
            joint.transform.localRotation = Quaternion.Lerp(joint.transform.localRotation, Quaternion.Euler(0, (float)angle, 0), moveSpeed);

            yield return null;
            if (state != MoveState.Rotate) yield break;
        }
    }


    public enum MoveState
    {
        Lock = 0,
        Rotate = 1,
        Unlock = 2,
        Recalibrate = 3
    }


}

