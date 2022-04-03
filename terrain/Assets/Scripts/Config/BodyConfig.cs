using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Config
{
    public class BodyConfig : JointConfig
    {
        public Vector3[] LegPoints = new Vector3[2];

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
        public Gene DriveVelocity = new Gene(1f, 0f, 1.5f, Variable.DriveVelocity);

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

        internal string GetHeader()
        {
            string line = "";
            line += $"Section No, ";
            line += $"{nameof(AngleConstraint)}, ";
            line += $"{nameof(RotationMultiplier)}, ";
            line += $"{nameof(IsRotating)}, ";
            line += $"{nameof(UseSin)}, ";
            line += $"{nameof(IsDriving)}, ";
            line += $"{nameof(DriveVelocity)}, ";
            line += $"{nameof(Size)}, ";
            line += $"{nameof(Mass)}, ";
            return line;
        }

        internal string GetData(int index)
        {
            string line = "";
            line += $"{index}, ";
            line += $"{Regex.Replace(AngleConstraint.Value.ToString(), @",\s", " - ")}, ";
            line += $"{Regex.Replace(RotationMultiplier.Value.ToString(), @",\s", " - ")}, ";
            line += $"{IsRotating.Value}, ";
            line += $"{UseSin.Value}, ";
            line += $"{IsDriving.Value}, ";
            line += $"{DriveVelocity.Value}, ";
            line += $"{Size.Value}, ";
            line += $"{Mass.Value}, ";
            return line;
        }

        internal string GetEmptyData(int index)
        {
            string line = "";
            line += $"{index}, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            return line;
        }
    }
}