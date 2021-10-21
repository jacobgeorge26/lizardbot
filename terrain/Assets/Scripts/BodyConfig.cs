using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : MonoBehaviour
    {
        //determines whether the joint will be rotating or not
        [Header("Rotation Setup")]

        [SerializeField]
        protected bool IsRotating;

        //which direction will the joint be updating this iteration
        [SerializeField]
        protected bool IsClockwise;

        //what is the max angle of the joint
        [SerializeField]
        [Range(0, 60)]
        protected int MaxAngle;

        //what is the velocity of the joint e.g. 100 deg/sec
        [SerializeField]
        [Range(60, 180)]
        protected int TurnVelocity;

        [Header("Drive Setup")]

        [SerializeField]
        protected bool IsDriving;

        [SerializeField]
        [Range(0.1f, 1f)]
        protected float DriveVelocity;

    }
}