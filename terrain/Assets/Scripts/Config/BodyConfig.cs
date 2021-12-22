using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : MonoBehaviour
    {
        [HideInInspector]
        public int Index = 0;

        //determines whether the joint will be rotating or not
        [Header("Rotation Setup")]

        //will this section be rotating (it can rotate AND drive if that is desirable)
        public bool IsRotating = true;

        /* if you change AngleConstraint's limits then check for strange behaviour
        * if too high it will break expected behvaiour from a physical robot
        * if negative then just what
        * if over 180 it will break GetRelativeAngle
        * so just pls keep it 0 <= x <= 179 at the very least and have a weird robot
        */

        //what is the angle constraint of the joint
        //lower -> tighter coil
        [Tooltip("It is recommended that the angle constraint is in range 30 <= x <= 120")]
        public int[] AngleConstraint = new int[3] { 120, 120, 120 };

        [Tooltip("Range 0 <= x <= 1")]
        public float[] RotationMultiplier = new float[3] { 0.5f, 1, 0.5f };

        [HideInInspector]
        public bool UseSin = true;

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        public bool IsDriving = true;

        //how fast should the section move forward
        //>1 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        [Range(0f, 3f)]
        public float DriveVelocity = 2f;

        public static void Copy(BodyConfig copyTo, BodyConfig copyFrom)
        {
            copyTo.Index = copyFrom.Index;
            copyTo.IsRotating = copyFrom.IsRotating;
            copyTo.AngleConstraint = copyFrom.AngleConstraint;
            copyTo.RotationMultiplier = copyFrom.RotationMultiplier;
            copyTo.UseSin = copyFrom.UseSin;
            copyTo.IsDriving = copyFrom.IsDriving;
            copyTo.DriveVelocity = copyFrom.DriveVelocity;
        }
    }
}