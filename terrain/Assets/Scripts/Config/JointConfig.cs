using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class JointConfig : object
    {
        /* if you change AngleConstraint's limits then check for strange behaviour
        * if too high it will break expected behaviour from a physical robot
        * if negative then just what
        * if over 180 it will break A LOT
        * so just pls keep it 0 <= x <= 180 at the very least and have a weird robot
        */
        //what is the angle constraint of the joint
        //lower -> tighter coil
        [Tooltip("It is recommended that the angle constraint is in range 30 <= x <= 90")]
        public Gene AngleConstraint = new Gene(new Vector3(120, 120, 120), 0, 180, Variable.AngleConstraint);

        //when rotating how much force should each axis have applied to it?
        //e.g. 0.5, 1, 0.5 makes y the primary axis
        [Tooltip("Range 0 <= x <= 1")]
        public Gene RotationMultiplier = new Gene(new Vector3(0.5f, 1f, 0.5f), 0f, 1f, Variable.RotationMultiplier);

        //used by dynamic movement, each index corresponds to the initial velocity that was used to move a certain direction
        public Vector3[] Velocities = new Vector3[DynMovConfig.NoSphereSamples];

        //store the initial velocity - used to see if this movement was more efficient than a previous
        public Vector3 CurrentVelocity = Vector3.zero;

    }
}