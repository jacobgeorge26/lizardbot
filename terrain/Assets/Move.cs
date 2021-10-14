using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;
using Helpers;

public class Move : BodyHelpers
{ 
   //Calculated
    private Vector3[] relativeAngles;

    //testing variables
    bool coiling = true;
    int sleeptime = 25;



    //To Do: when JointConfig is working use queue and limit to unlocked joints only
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    void Update()
    {
        if(coiling)
        {
            bool coilingChanged = coiling;

            if (coiling) ToggleBodyLock(0, true, LockOption.Backward); else ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward);

            relativeAngles = GetRelativeRotations();

            for (int i = 0; i < Joints.Count; i++)
            {
                int index = coiling ? i : Joints.Count - 1 - i;

                if (!Locked[index])
                {
                    GameObject joint = Joints[index];

                    double angle = (coiling && Direction[index]) || (!coiling && Math.Round(relativeAngles[index].y, 0) < 0) ? maxAngle : maxAngle * -1;

                    StartCoroutine(RotateJoint(joint, maxAngle));

                    relativeAngles = GetRelativeRotations();
                    printRelativeAngles(relativeAngles);

                    //joint.transform.Rotate(0, (float)angle, 0);

                    //relativeAngles[index].y += (float)angle; //update relative angles for the coiling validation to work

                    //System.Threading.Thread.Sleep(sleeptime);
                }
            }

            coiling = coiling ? Math.Abs(Math.Round(relativeAngles[0].y, 0)) < maxAngle : !(relativeAngles.Any(j => Math.Abs(Math.Round(j.y, 0)) > 0 && Math.Abs(Math.Round(j.y, 0)) != 180));

            if (coiling != coilingChanged) //coiling / uncoiling has finished and will reverse next frame
            {
                relativeAngles = GetRelativeRotations();
                printRelativeAngles(relativeAngles);

                ToggleBodyLock(0, false, LockOption.Backward);

                RecalibrateJoints();
            }
            else
            {
                ToggleBodyLock(0, false, LockOption.Backward);
            }
        }

    }

    IEnumerator RotateJoint(GameObject joint, double angle)
    {
        float moveSpeed = 0.2f;
        while (joint.transform.rotation.y < angle)
        {
            joint.transform.rotation = Quaternion.Lerp(joint.transform.rotation, Quaternion.Euler(0, (float)angle, 0), moveSpeed);
            yield return new WaitForSeconds(0.5f);
        }
    }


    //TO DO: update for 3D recal
    private void RecalibrateJoints()
    {
        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];
            double recalAngle = 0;        
            if (!Locked[index])
            {
                if (!coiling)
                {
                    recalAngle = maxAngle; //turn again such that after the next turn it will be facing toward the coupled section
                    recalAngle *= Direction[index] ? -1 : 1; //which direction should it turn
                }
            }
            else
            {
                recalAngle = Math.Round(relativeAngles[index].y, 0) > 0 ? relativeAngles[index].y * -1 : 0;
            }
            joint.transform.Rotate(0, (float)recalAngle, 0);
            System.Threading.Thread.Sleep(sleeptime);
        }
    }




}

