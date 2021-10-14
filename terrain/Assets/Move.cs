using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Body;

public class Move : MonoBehaviour
{
    //Objects
    [Header("Game Objects")]

    [SerializeField]
    private GameObject Head;
    [SerializeField]
    private List<GameObject> Joints;
    [SerializeField]
    private List<GameObject> Sections;
 
   //Calculated
    private Vector3[] relativeAngles;

    //TO DO: convert to JointSetup where:
    [Header("Joint Setup")]
    [SerializeField]    
    [Range(5, 20)]
    private double coilAngle;

    [SerializeField]
    
    [Range(5, 60)]
    private double maxAngle;

    [SerializeField]
    private bool[] Locked;

    [SerializeField]
    private bool[] Direction; //true = clockwise rotation when coiling

    //testing variables
    bool coiling = true;
    int sleeptime = 25;


    //To Do: when JointConfig is working use queue and limit to unlocked joints only
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    void Update()
    {
        bool coilingChanged = coiling;

        if (coiling)
        {
            ToggleBodyLock(0, true, LockOption.Backward);
        }

        else
        {
            ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward);
        }

        relativeAngles = GetRelativeRotations();

        for (int i = 0; i < Joints.Count; i++)
        {
            int index = coiling ? i : Joints.Count - 1 - i;
            if (!Locked[index])
            {
                GameObject joint = Joints[index];

                double angle = (coiling && Direction[index]) || (!coiling && Math.Round(relativeAngles[index].y, 0) < 0) ? coilAngle : coilAngle * -1;

                joint.transform.Rotate(0, (float)angle, 0);
                relativeAngles[index].y += (float)angle; //update relative angles for the coiling validation to work

                System.Threading.Thread.Sleep(sleeptime);
            }
        }

        coiling = coiling ? Math.Abs(Math.Round(relativeAngles[0].y, 0)) < maxAngle : !(relativeAngles.Any(j => Math.Abs(Math.Round(j.y, 0)) > 0 && Math.Abs(Math.Round(j.y, 0)) != 180));

        if (coiling != coilingChanged) //coiling / uncoiling has finished and will reverse next frame
        {
            relativeAngles = GetRelativeRotations();
            printRelativeAngles();

            ToggleBodyLock(0, false, LockOption.Backward);

            RecalibrateJoints();
        }
        else
        {
            ToggleBodyLock(0, false, LockOption.Backward);
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

    private void printRelativeAngles()
    {
        string message = "";
        relativeAngles.ToList().ForEach(j => message += $"{j.y.ToString("N0")};    ");
        Debug.Log(message);       
    }

    //MUST be locked to work
    private Vector3[] GetRelativeRotations()
    {
        Vector3[] rotations = new Vector3[Joints.Count];
        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];

            if (!Locked[index]) //this joint is not locked and will be rotating
            {
                rotations[index] = joint.transform.localEulerAngles;
                rotations[index].y -= Math.Round(rotations[index].y, 0) > 180 ? 360 : 0;
                rotations[index].y += Math.Round(rotations[index].y, 0) < -180 ? 360 : 0;
            }        
        }
        return rotations;
    }

    private void ToggleBodyLock(int bodyIndex, bool lockBody, LockOption direction)
    {
        //validation
        if (bodyIndex < 0 || bodyIndex >= Joints.Count)
        {
            Debug.LogError($"Index {bodyIndex} is out of range in ToggleBodyLock");
        }

        if (direction == LockOption.Backward)
        {
            for (int index = bodyIndex; index < Joints.Count; index++)
            {
                Joints[index].transform.parent = lockBody ? (index == 0 ? Head.transform : Sections[index - 1].transform) : this.transform;
                Sections[index].transform.parent = lockBody ? Joints[index].transform : this.transform;
            }
        }
        else if(direction == LockOption.Forward)
        {
            for (int index = bodyIndex; index >= 0; index--)
            {
                Joints[index].transform.parent = lockBody ? Sections[index].transform : this.transform;
                Sections[index].transform.parent = index < Joints.Count - 1 && lockBody ?  Joints[index + 1].transform : this.transform;
            }
            Head.transform.parent = lockBody ? Joints[0].transform : this.transform;
        }


    }

    enum LockOption
    {
        Forward = 0,
        Backward = 1
    }

}

