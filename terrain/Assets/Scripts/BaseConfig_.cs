using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class BaseConfig : object
    {
        public static bool isDefault { get; set; } = true;

        public static int NoSections { get; set; } = -1;

        public static int DefaultNoSections { get; set; } = 5;

        public static List<GameObject> Sections { get; set; } = new List<GameObject>();

        /*-------------------------------------------------------------------------------------------------------*/

        public static bool[] RotatingSections { get; set; }

        public static bool[] DrivingSections { get; set; }

        //what is the velocity of the turn e.g. 100 deg/sec
        //lower -> tighter coil
        /*Recommend a velocity around 180*/
        public static int TurnVelocity { get; set; }


        //how fast should the section move forward
        //>1 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        public static float DriveVelocity { get; set; }

        /* FOR BELOW MAXANGLE
         * if you change MaxAngle's limits then check for strange behaviour
         * if too high it will break expected behvaiour from a physical robot
         * if negative then just what
         * if over 180 it will break GetRelativeAngle
         * so just pls keep it 0 <= x <= 179 at the very least and have a weird robot
         */

        //what is the max angle of the joint
        //lower -> tighter coil
        public static int[] MaxAngle { get; set; } = new int[3];

    }
}


