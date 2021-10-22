//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Config;
//using System;

/*
 * ------------------------
 * This is the script for:
 * when I was initially moving the entire body at the same time - this was the helper class that did basic methods
 * ------------------------
 */

//namespace Helpers
//{
//    public enum LockOption
//    {
//        Forward = 0,
//        Backward = 1
//    }

//    public class BodyHelpers : EntireBodyConfig
//    {
//        protected double AbsRnd(float angle)
//        {
//            return Math.Abs(Math.Round(angle, 0));
//        }

//        protected void printRelativeAngles(Vector3[] relativeAngles)
//        {
//            string message = "";
//            relativeAngles.ToList().ForEach(j => message += $"{j.y.ToString("N0")};    ");
//            Debug.Log(message);
//        }

//        protected IEnumerator ToggleBodyLock(int bodyIndex, bool lockBody, LockOption direction)
//        {
//            //validation
//            if (bodyIndex < 0 || bodyIndex >= Joints.Count)
//            {
//                Debug.LogError($"Index {bodyIndex} is out of range in ToggleBodyLock");
//            }

//            if (direction == LockOption.Backward)
//            {
//                for (int index = bodyIndex; index < Joints.Count; index++)
//                {
//                    Joints[index].transform.parent = (index == 0 ? Head.transform : Sections[index - 1].transform);
//                    //Joints[index].transform.parent = lockBody ? (index == 0 ? Head.transform : Sections[index - 1].transform) : this.transform;
//                    Sections[index].transform.parent = lockBody ? Joints[index].transform : this.transform;
//                }
//            }
//            else if (direction == LockOption.Forward)
//            {
//                for (int index = bodyIndex; index >= 0; index--)
//                {
//                    Joints[index].transform.parent = lockBody ? Sections[index].transform : this.transform;
//                    Sections[index].transform.parent = index < Joints.Count - 1 && lockBody ? Joints[index + 1].transform : this.transform;
//                }
//                Head.transform.parent = lockBody ? Joints[0].transform : this.transform;
//            }
//            yield return null;
//        }

//        //MUST be locked to work
//        protected Vector3[] GetRelativeRotations()
//        {
//            Vector3[] rotations = new Vector3[Joints.Count];
//            for (int index = 0; index < Joints.Count; index++)
//            {
//                GameObject joint = Joints[index];

//                if (!Locked[index]) //this joint is not locked and will be rotating
//                {
//                    rotations[index] = joint.transform.localEulerAngles;
//                    rotations[index].y -= Math.Round(rotations[index].y, 0) > 180 ? 360 : 0;
//                    rotations[index].y += Math.Round(rotations[index].y, 0) < -180 ? 360 : 0;
//                }
//            }
//            return rotations;
//        }


//        protected Vector3 GetAngle(GameObject joint)
//        {
//            Vector3 angle = new Vector3(0, 0, 0);
//                angle = joint.transform.localEulerAngles;
//                angle.y -= Math.Round(angle.y, 0) > 180 ? 360 : 0;
//                angle.y += Math.Round(angle.y, 0) < -180 ? 360 : 0;
//                return angle;
//        }


//    }

//}

