using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ImportantStuff;

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
    bool uncoiling = true;
    int iterations = 10;
    int sleeptime = 25;


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
        if (coiling)
        {            
            Coil();
        }
        else if (uncoiling)
        {
            Uncoil();
        }
    }

    //iterate through each joint / section and rotate it
    //every other joint will not rotate to produce a wide coil
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    private void Coil()
    {
        ToggleBodyLock(0, true, LockOption.Backward);

        relativeAngles = GetRelativeRotations();

        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints[index];

            if (!Locked[index])
            {
                double angle = Direction[index] ? coilAngle : coilAngle * -1;
                joint.transform.Rotate(0, (float)angle, 0);
                System.Threading.Thread.Sleep(sleeptime);              
            }  
        }
        relativeAngles = GetRelativeRotations();
        printRelativeAngles();

        ToggleBodyLock(0, false, LockOption.Backward);


        coiling = Math.Abs(Math.Round(relativeAngles[0].y, 0)) < maxAngle;

        if (!coiling)
        {
            RecalibrateJoints();
            Debug.Log("-----Uncoiling-----");
        }

    }

    private void Uncoil()
    {
        ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward);

        for (int index = Joints.Count - 1; index >= 0; index--)
        {
            GameObject joint = Joints[index];

            if(!Locked[index] && Math.Round(Math.Abs(relativeAngles[index].y), 0) != 0)
            {
                double angle = Math.Round(relativeAngles[index].y, 0) < 0 ? coilAngle : -1 * coilAngle;

                joint.transform.Rotate(0, (float)angle, 0);

                System.Threading.Thread.Sleep(sleeptime);             
            }

        }
        relativeAngles = GetRelativeRotations();
        printRelativeAngles();

        ToggleBodyLock(Joints.Count - 1, false, LockOption.Forward);     

        uncoiling = relativeAngles.Any(j => Math.Abs(Math.Round(j.y, 0)) > 0 && Math.Abs(Math.Round(j.y, 0)) != 180);

        if (!uncoiling)
        {
            RecalibrateJoints();

            Debug.Log("-----Coiling-----");
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
    //TO DO: simplify this, first two can be merged
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
        else if (direction == LockOption.Unlocked)
        {
            for (int index = bodyIndex; index >= 0; index--)
            {
                Joints[index].transform.parent = this.transform;
                Sections[index].transform.parent =  this.transform;
            }
            Head.transform.parent =  this.transform;
        }

    }

    enum LockOption
    {
        Forward = 0,
        Backward = 1,
        Unlocked = 2
    }

}

