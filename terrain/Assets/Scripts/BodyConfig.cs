using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : MonoBehaviour
    {
        //determines whether the joint will be rotating or not
        [Header("Rotation Setup")]

        //will this section be rotating (it can rotate AND drive if that is desirable)
        public bool IsRotating = true;

        //which direction will the joint be updating this iteration
        public bool[] IsClockwise = new bool[3] { false, true, false };

        /* if you change MaxAngle's limits then check for strange behaviour
        * if too high it will break expected behvaiour from a physical robot
        * if negative then just what
        * if over 180 it will break GetRelativeAngle
        * so just pls keep it 0 <= x <= 179 at the very least and have a weird robot
        */

        //what is the max angle of the joint
        //lower -> tighter coil
        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        public int[] MaxAngle = new int[3] { 30, 45, 45 };

        [Tooltip("Range 0 <= x <= 1")]
        public float[] TurnRatio = new float[3] { 0.5f, 1f, 0.5f };

        //what is the velocity of the turn e.g. 100 deg/sec
        //lower -> tighter coil
        /*Recommend a velocity around 180*/
        [Range(0, 360)]
        public int TurnVelocity = 180;

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        public bool IsDriving = false;

        //how fast should the section move forward
        //>1 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        [Range(0f, 3f)]
        public float DriveVelocity = 2f;

    }
}