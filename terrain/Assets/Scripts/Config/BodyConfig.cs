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

        //will this section use sin or cos in the oscillation?
        [HideInInspector]
        public bool UseSin = true;

        public JointConfig JointConfig = new JointConfig(null, null);

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        public bool IsDriving = true;

        //how fast should the section move forward
        //>3 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        //<1 is too slow and will trigger the robot being stuck more easily - especially in the rough terrain
        [Range(0, 3)]
        private static float defaultDriveVelocity = 2f;
        public RangedVariable DriveVelocity = new RangedVariable(defaultDriveVelocity, 0f, 3f);

        public static void Copy(BodyConfig copyTo, BodyConfig copyFrom)
        {
            copyTo.Index = copyFrom.Index;
            copyTo.IsRotating = copyFrom.IsRotating;
            copyTo.JointConfig.AngleConstraint = copyFrom.JointConfig.AngleConstraint;
            copyTo.JointConfig.RotationMultiplier = copyFrom.JointConfig.RotationMultiplier;
            copyTo.UseSin = copyFrom.UseSin;
            copyTo.IsDriving = copyFrom.IsDriving;
            copyTo.DriveVelocity = copyFrom.DriveVelocity;
        }
    }
}