using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ImportantStuff;

public class Move : MonoBehaviour
{
    public GameObject Head;
    public List<GameObject> Joints;
    public List<GameObject> Sections;
    public GameObject Camera;
    bool coiling = true;
    bool uncoiling = true;
    
    private Vector3[] relativeAngles;
    private int iterations = 10;

    //TO DO: convert to JointSetup where:
    private double coilAngle = 10;
    private readonly double maxAngle = 45;
    private bool[] Locked = { false, true, false, true };

    // Start is called before the first frame update
    //TO DO: calculate centre of gravity
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //TO DO: turn to flip rather than coil / uncoil
        if (!coiling && !uncoiling && iterations > 0)
        {
            coiling = true;
            uncoiling = true;
            iterations--;
        }
        if (coiling) Coil();
        else if (uncoiling) Uncoil();
    }

    //iterate through each joint / section and rotate it
    //every other joint will not rotate to produce a wide coil
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    private void Coil()
    {
        relativeAngles = GetRelativeRotations();

        ToggleBodyLock(0, true, LockOption.Backward);

        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];

            if (index % 2 == 0)
            {
                double angle = index == 0 ? coilAngle : -1 * coilAngle;
                joint.transform.localEulerAngles = new Vector3(0, joint.transform.localEulerAngles.y + (float)angle, 0);
                System.Threading.Thread.Sleep(25);              
            }  
        }

        ToggleBodyLock(0, false, LockOption.Backward);

        relativeAngles = GetRelativeRotations();

        coiling = Math.Abs(Math.Round(relativeAngles[0].y, 0)) < maxAngle;

        if (!coiling)
        {
            RecalibrateJoints();
        }


    }

    private void Uncoil()
    {     
        ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward);

        for (int index = Joints.Count - 1; index >= 0; index--)
        {
            GameObject joint = Joints[index];

            //TO DO: remove the index logic
            if(index % 2 == 0)
            {
                double angle = index == 0 ?  coilAngle : -1 * coilAngle;
                joint.transform.localEulerAngles = new Vector3(0, joint.transform.localEulerAngles.y + (float)angle, 0);
                System.Threading.Thread.Sleep(25);
             
            }

        }
        ToggleBodyLock(Joints.Count - 1, false, LockOption.Forward);     

        relativeAngles = GetRelativeRotations();

        uncoiling = relativeAngles.Any(j => Math.Round(j.y, 0) != 0);

        if (!uncoiling)
        {
            RecalibrateJoints();
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
                if (uncoiling && !coiling)
                {
                    recalAngle = maxAngle; //turn again such that after the next turn it will be facing toward the coupled section
                    recalAngle *= index == 0 ? -1 : 1; //which direction should it turn
                }
            }
            else
            {
                recalAngle = Math.Round(relativeAngles[index].y, 0) > 0 ? 180 : -180;
            }
            joint.transform.Rotate(0, (float)recalAngle, 0);
        }
    }

    private void printRelativeAngles()
    {
        string message = "";
        for (int i = 0; i < Joints.Count; i++)
        {
            message += $"{relativeAngles[i].y.ToString("N0")};   ";
        }
        Debug.Log(message);       
    }

    //MUST be locked in coil to work
    //TO DO: simplify this, first two can be merged
    private Vector3[] GetRelativeRotations()
    {
        Vector3[] rotations = new Vector3[Joints.Count];
        Vector3 last = new Vector3(0, 0, 0);
        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];

            if (!Locked[index]) //this joint is not locked and will be rotating
            {
                rotations[index] = joint.transform.localRotation.eulerAngles;
                //rotations[index].y -= Math.Round(rotations[index].y, 0) > 180 ? 360 : 0;
                //rotations[index].y += Math.Round(rotations[index].y, 0) < -180 ? 360 : 0;
                if (Math.Round(last.y, 0) != Math.Round(rotations[index].y, 0))
                {
                    var temp = last; //need to store last as the eulerAngle not relative angle
                    last = rotations[index];
                    rotations[index].y -= temp.y;
                }
            }        
        }
        return rotations;
    }

    private void ToggleBodyLock(int bodyIndex, bool lockBody, LockOption direction)
    {
        if (direction == LockOption.Backward)
        {
            for (int index = bodyIndex; index < Joints.Count; index++)
            {
                Joints[index].transform.parent = index == 0 && lockBody ? Head.transform : this.transform;
                Joints[index].transform.parent = index > 0 && lockBody ? Sections[index - 1].transform : this.transform;
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
        Backward = 1,
        Unlocked = 2
    }

}

