using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject Head;
    public List<GameObject> Joints;
    public List<GameObject> Sections;
    public GameObject Camera;
    bool coiling = true;
    bool uncoiling = true;
    private double coilAngle = 10;
    private int turncount = 12;

    private bool uncoiltest = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        if (coiling) Coil();
        //else if (uncoiling) Uncoil();
    }

    //iterate through each joint / section and rotate it
    //every other joint will not rotate to produce a wide coil
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    private void Coil()
    {     
        ToggleBodyLock(0, true, LockOption.Backward);
        for (int index = 0; index < Joints.Count; index++)
        {
            if (index % 2 == 0)
            {
                GameObject joint = Joints[index];
                double angle = index % 4 == 0 ? coilAngle * -0.5 : coilAngle;
                joint.transform.Rotate(0, (float)angle, 0, Space.Self);
                turncount--;
                System.Threading.Thread.Sleep(25);                    
            }  
        }
        coiling = turncount > 0;
        ToggleBodyLock(0, false, LockOption.Backward);
    }

    private void Uncoil()
    {
        Vector3[] relativeAngles = new Vector3[Joints.Count];
        for (int joint = 0; joint < Joints.Count; joint++)
        {
            relativeAngles[joint] = GetRelativeRotation(joint);
            Debug.Log($"Joint: {joint + 1}; Angle: {relativeAngles[joint].y}");
        }
        ToggleBodyLock(Joints.Count - 1, true, LockOption.Forward);       

        for (int index = Joints.Count - 1; index >= 0; index--)
        {
            GameObject joint = Joints[index];

            if (Math.Abs(relativeAngles[index].y) > coilAngle)
            {
                double angle = coilAngle;
                //double angle = joint.transform.rotation.y > 0 ? coilAngle * -1 : coilAngle;
                int count = 0;

                while (Math.Abs(relativeAngles[index].y) > Quaternion.Euler(0, (float)coilAngle, 0).y && count < 100)
                {
                    joint.transform.Rotate(0, (float)angle, 0, Space.Self);
                    relativeAngles[index].y += (float)angle;
                    System.Threading.Thread.Sleep(25);
                    count++;
                }

            }

        }
        uncoiling = false;
        //uncoiling = Math.Abs(Joints[0].transform.rotation.y) > Quaternion.Euler(0, (float)coilAngle, 0).y; //do once while testing

        ToggleBodyLock(Joints.Count - 1, false, LockOption.Forward);
    }

    //MUST be unlocked to work
    private Vector3 GetRelativeRotation(int index)
    {
        Vector3 relativeAngles = new Vector3(0, 0, 0);
        GameObject joint = Joints[index];
        GameObject section = index > 0 ? Sections[index - 1] : Head;      
        relativeAngles.y = 360 - section.transform.rotation.eulerAngles.y;
        relativeAngles.y = relativeAngles.y > 180 ? relativeAngles.y - 360 : relativeAngles.y;        
        return relativeAngles;
    }

    private void ToggleBodyLock(int bodyIndex, bool lockBody, LockOption direction)
    {
        if (direction == LockOption.Backward)
        {
            Camera.transform.parent = Head.transform;
            for (int index = bodyIndex; index < Joints.Count; index++)
            {
                Joints[index].transform.parent = index > 0 && lockBody ? Sections[index - 1].transform : this.transform;
                Sections[index].transform.parent = lockBody ? Joints[index].transform : this.transform;
            }
        }
        else if(direction == LockOption.Forward)
        {
            Camera.transform.parent = Sections.Last().transform;
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
