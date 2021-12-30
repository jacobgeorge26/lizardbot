using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class JointConfig : MonoBehaviour
    {
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

        //when rotating how much force should each axis have applied to it?
        //e.g. 0.5, 1, 0.5 makes y the primary axis
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
    }
}