using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BaseConfig : MonoBehaviour
    {

        //TODO: editor settings window
        //allow more customisation for the setup - select number of sections
        //then for each section it shows options for each one
        //full config - add weight - add gravity

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

        /* FOR BELOW MAXANGLE
         * if you change MaxAngle's limits then check for strange behaviour
         * if too high it will break expected behvaiour from a physical robot
         * if negative then just what
         * if over 180 it will break GetRelativeAngle
         * so just pls keep it 0 <= x <= 179 at the very least and have a weird robot
         */

        //what is the max angle of the joint
        //lower -> tighter coil
        [SerializeField]
        [Range(0, 180)]
        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        protected int X_MaxAngle;

        [SerializeField]
        [Range(0, 180)]
        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        protected int Y_MaxAngle;

        [SerializeField]
        [Range(0, 180)]
        [Tooltip("It is recommended that the max angle is in range 0 <= x <= 60")]
        protected int Z_MaxAngle;


        protected int[] MaxAngle = new int[3];

        private void Start()
        {
            MaxAngle = new int[3]{ X_MaxAngle, Y_MaxAngle, Z_MaxAngle};
        }
    }
}


