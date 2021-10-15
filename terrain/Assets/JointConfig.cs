using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class JointConfig : MonoBehaviour
    {
        [Header("Game Objects")]
        [SerializeField]
        protected GameObject Joint;

        [SerializeField]
        protected GameObject PreviousSection;

        [SerializeField]
        protected GameObject NextSection;

        [Header("Joint Setup")]
        [SerializeField]
        protected bool IsLocked;

        [SerializeField]
        protected bool IsClockwise;

        [SerializeField]
        protected int MaxAngle;

        protected Vector3 RelativeAngle;

    }
}

