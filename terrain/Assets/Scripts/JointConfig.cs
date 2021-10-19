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

        //the joint will by default be locked to the previous section, and switch to next section whilst uncoiling
        [SerializeField]
        protected GameObject PreviousSection;

        [SerializeField]
        protected GameObject NextSection;

        //determines whether the joint will be rotating or not
        [Header("Joint Setup")]
        [SerializeField]
        protected bool IsLocked;

        //which direction will the joint be updating this iteration
        [SerializeField]
        protected bool IsClockwise;

        //what is the max angle of the joint
        [SerializeField]
        [Range(0, 60)]
        protected int MaxAngle;


    }
}

