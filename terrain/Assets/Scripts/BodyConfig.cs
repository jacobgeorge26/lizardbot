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
        [SerializeField]
        public bool IsRotating;

        //which direction will the joint be updating this iteration
        [SerializeField]
        public bool[] IsClockwise;

        private int[] maxAngle;
        [HideInInspector]
        public int[] MaxAngle
        {
            get { return maxAngle; }
            set { maxAngle = value; }
        }

        
        private int turnVelocity;
        [HideInInspector]
        public int TurnVelocity
        {
            get { return turnVelocity; }
            set { turnVelocity = value; }
        }

        /*-------------------------------------------------------------------------------------------------------*/


        [Header("Drive Setup")]

        //will this section be driving (it can rotate AND drive if that is desirable)
        [SerializeField]
        public bool IsDriving;

        
        private float driveVelocity;
        [HideInInspector]
        public float DriveVelocity
        {
            get { return driveVelocity; }
            set { driveVelocity = value; }
        }
    }
}