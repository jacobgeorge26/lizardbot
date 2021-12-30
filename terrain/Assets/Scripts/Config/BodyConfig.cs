using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : JointConfig
    {
        [HideInInspector]
        public int Index = 0;

        //determines whether the joint will be rotating or not
        [Header("Rotation Setup")]

        //will this section be rotating (it can rotate AND drive if that is desirable)
        public bool IsRotating = true;

        //will this section use sin or cos in the oscillation?
        [HideInInspector]
        public bool UseSin = true;

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        public bool IsDriving = true;

        //how fast should the section move forward
        //>3 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        //<1 is too slow and will trigger the robot being stuck more easily - especially in the rough terrain
        [Range(0, 3)]
        private float _driveVelocity = 2f;
        private float driveMin = 0, driveMax = 3;
        public float DriveVelocity
        {
            get => _driveVelocity;
            set
            {
                if (value < driveMin)
                    _driveVelocity = driveMin;
                else if (value > driveMax)
                    _driveVelocity = driveMax;
                else
                    _driveVelocity = value;
            }
        }

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