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
        * so just pls keep it 0 <= x <= 180 at the very least and have a weird robot
        */

        //what is the angle constraint of the joint
        //lower -> tighter coil
        [Tooltip("It is recommended that the angle constraint is in range 30 <= x <= 120")]
        private int[] _angleConstraint = new int[3] { 120, 120, 120 };

        private int angleMin = 0, angleMax = 180;
        public int[] AngleConstraint
        {
            get => _angleConstraint;
            set
            {
                for (int i = 0; i < 3; i++)
                {
                    if (value[i] < angleMin)
                        _angleConstraint[i] = angleMin;
                    else if (value[i] > angleMax)
                        _angleConstraint[i] = angleMax;
                    else
                        _angleConstraint = value;
                }
            }
        }

        [Tooltip("Range 0 <= x <= 1")]
        private float[] _rotationMultiplier = new float[3] { 0.5f, 1, 0.5f };

        private float rmMin = 0, rmMax = 1;
        public float[] RotationMultiplier
        {
            get => _rotationMultiplier;
            set
            {
                for (int i = 0; i < 3; i++)
                {
                    if (value[i] < rmMin)
                        _rotationMultiplier[i] = rmMin;
                    else if (value[i] > rmMax)
                        _rotationMultiplier[i] = rmMax;
                    else
                        _rotationMultiplier = value;
                }
            }
        }

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