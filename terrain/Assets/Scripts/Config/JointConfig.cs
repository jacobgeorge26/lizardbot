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
        public RangedVariable AngleConstraint = new RangedVariable(new Vector3(120, 120, 120), 0, 180);

        //when rotating how much force should each axis have applied to it?
        //e.g. 0.5, 1, 0.5 makes y the primary axis
        [Tooltip("Range 0 <= x <= 1")]
        public RangedVariable RotationMultiplier = new RangedVariable(new Vector3(0.5f, 1f, 0.5f), 0f, 1f);

    }
}