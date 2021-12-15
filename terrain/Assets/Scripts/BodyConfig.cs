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
        public bool IsRotating;

        //which direction will the joint be updating this iteration
        public bool[] IsClockwise;

        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        public int[] MaxAngle;

        [Tooltip("Range 0 <= x <= 1")]
        public float[] TurnRatio;

        [Range(0, 360)]
        public int TurnVelocity;

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        public bool IsDriving;

        [Range(0f, 1f)]
        public float DriveVelocity;

    }
}