using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class TailConfig : JointConfig
    {
        public Gene TailMassMultiplier = new Gene(1f, 0.2f, 1.5f, Variable.TailMassMultiplier);

        public Gene Length = new Gene(2f, 1f, 4f, Variable.Length);

        public void Clone(TailConfig oldConfig)
        {
            AngleConstraint.Value = oldConfig.AngleConstraint.Value;
            RotationMultiplier.Value = oldConfig.RotationMultiplier.Value;
            TailMassMultiplier.Value = oldConfig.TailMassMultiplier.Value;
        }

        internal string GetHeader()
        {
            string line = "";
            line += $"Tail No, ";
            line += $"{nameof(AngleConstraint)}, ";
            line += $"{nameof(RotationMultiplier)}, ";
            line += $"{nameof(TailMassMultiplier)}, ";
            line += $"{nameof(Length)}, ";
            return line;
        }

        internal string GetData()
        {
            string line = "";
            line += $"0, ";
            line += $"\"{AngleConstraint.Value}\", ";
            line += $"\"{RotationMultiplier.Value}\", ";
            line += $"\"{TailMassMultiplier.Value}\", ";
            line += $"{Length.Value}, ";
            return line;
        }

        internal string GetEmptyData()
        {
            string line = "";
            line += $"0, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            line += $"-, ";
            return line;
        }
    }
}

