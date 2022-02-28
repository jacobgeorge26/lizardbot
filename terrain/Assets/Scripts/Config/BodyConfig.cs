using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class BodyConfig : JointConfig
    {

        //will this section be rotating (it can rotate AND drive if that is desirable)
        public Gene IsRotating = new Gene(true, Variable.IsRotating);

        //will this section use sin or cos in the oscillation?
        [HideInInspector]
        public Gene UseSin = new Gene(true, Variable.UseSin);

        /*-------------------------------------------------------------------------------------------------------*/

        //will this section be driving (it can rotate AND drive if that is desirable)
        public Gene IsDriving = new Gene(true, Variable.IsDriving);

        //how fast should the section move forward
        //>3 is too fast and can be hard to follow / limit the effects of rotation as it's constantly just bouncing off the terrain
        //<1 is too slow and will trigger the robot being stuck more easily - especially in the rough terrain
        [Range(0, 3)]
        public Gene DriveVelocity = new Gene(1f, 0f, 2f, Variable.DriveVelocity);

        /*-------------------------------------------------------------------------------------------------------*/

        public Gene Size = new Gene(1f, 0.5f, 1.5f, Variable.Size);

        public Gene Mass = new Gene(1f, 0.5f, 1.5f, Variable.Mass);

        public void Clone(BodyConfig oldConfig)
        {
            AngleConstraint.Value = oldConfig.AngleConstraint.Value;
            RotationMultiplier.Value = oldConfig.RotationMultiplier.Value;
            IsRotating.Value = oldConfig.IsRotating.Value;
            UseSin.Value = oldConfig.UseSin.Value;
            IsDriving.Value = oldConfig.IsDriving.Value;
            DriveVelocity.Value = oldConfig.DriveVelocity.Value;
            Size.Value = oldConfig.Size.Value;
            Mass.Value = oldConfig.Mass.Value;
        }

        public void UpdateGene(Gene newGene)
        {
            switch (newGene.Type)
            {
                case Variable.AngleConstraint:
                    AngleConstraint = newGene;
                    break;
                case Variable.RotationMultiplier:
                    RotationMultiplier = newGene;
                    break;
                case Variable.IsRotating:
                    IsRotating = newGene;
                    break;
                case Variable.UseSin:
                    UseSin = newGene;
                    break;
                case Variable.IsDriving:
                    IsDriving = newGene;
                    break;
                case Variable.DriveVelocity:
                    DriveVelocity = newGene;
                    break;
                case Variable.Size:
                    Size = newGene;
                    break;
                case Variable.Mass:
                    Mass = newGene;
                    break;
                default:
                    throw new Exception("There was an issue updating the body config. Invalid gene type.");
            }
        }
    }
}