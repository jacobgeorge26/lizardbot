using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;
using System;

namespace Helpers
{
    public enum LockOption
    {
        Forward = 0,
        Backward = 1
    }

    public class BodyHelpers : BodyConfig
    {
        public void printRelativeAngles(Vector3[] relativeAngles)
        {
            string message = "";
            relativeAngles.ToList().ForEach(j => message += $"{j.y.ToString("N0")};    ");
            Debug.Log(message);
        }

        protected void ToggleBodyLock(int bodyIndex, bool lockBody, LockOption direction)
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
            else if (direction == LockOption.Forward)
            {
                for (int index = bodyIndex; index >= 0; index--)
                {
                    Joints[index].transform.parent = lockBody ? Sections[index].transform : this.transform;
                    Sections[index].transform.parent = index < Joints.Count - 1 && lockBody ? Joints[index + 1].transform : this.transform;
                }
                Head.transform.parent = lockBody ? Joints[0].transform : this.transform;
            }
        }

        //MUST be locked to work
        protected Vector3[] GetRelativeRotations()
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


    }

}

