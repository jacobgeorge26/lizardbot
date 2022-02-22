using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : JointConfig
    {

        //will this section be rotating (it can rotate AND drive if that is desirable)
        public GeneVariable IsRotating = new GeneVariable(true, Variable.IsRotating);

        //will this section use sin or cos in the oscillation?
        [HideInInspector]
        public GeneVariable UseSin = new GeneVariable(true, Variable.UseSin);

        /*-------------------------------------------------------------------------------------------------------*/

        //will this section be driving (it can rotate AND drive if that is desirable)
        public GeneVariable IsDriving = new GeneVariable(true, Variable.IsDriving);

        //how fast should the section move forward
        //>3 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        //<1 is too slow and will trigger the robot being stuck more easily - especially in the rough terrain
        [Range(0, 3)]
        public GeneVariable DriveVelocity = new GeneVariable(2f, 0f, 3f, Variable.DriveVelocity);

        /*-------------------------------------------------------------------------------------------------------*/

        public GeneVariable Size = new GeneVariable(1f, 0.5f, 1.5f, Variable.Size);

        public GeneVariable Mass = new GeneVariable(1f, 0.5f, 1.5f, Variable.Mass);
    }
}