using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BaseConfig : MonoBehaviour
    {
        [Header("Robot Setup")]
        [SerializeField]
        protected int NoSections;

        protected List<GameObject> Sections = new List<GameObject>();

        /*-------------------------------------------------------------------------------------------------------*/

        [Header("Movement Setup")]

        [SerializeField]
        protected bool[] RotatingSections;

        [SerializeField]
        protected bool[] DrivingSections;

        //what is the velocity of the turn e.g. 100 deg/sec
        //lower -> tighter coil
        /*Recommend a velocity around 180*/
        [SerializeField]
        [Range(0, 360)]
        protected int TurnVelocity;


        //how fast should the section move forward
        //>1 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        [SerializeField]
        [Range(0f, 1f)]
        protected float DriveVelocity;



        //what is the max angle of the joint
        //lower -> tighter coil
        [SerializeField]
        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        protected int[] MaxAngle;
        /*
         * if you change MaxAngle's limits then check for strange behaviour
         * if too high it will break expected behvaiour from a physical robot
         * if negative then just what
         * if over 180 it will break GetRelativeAngle
         * so just pls keep it 0 <= x <= 179 at the very least and have a weird robot
         */
    }
}


