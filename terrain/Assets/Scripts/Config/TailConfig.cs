using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            line += $"{Regex.Replace(AngleConstraint.Value.ToString(), @",\s", " - ")}, ";
            line += $"{Regex.Replace(RotationMultiplier.Value.ToString(), @",\s", " - ")}, ";
            line += $"{TailMassMultiplier.Value}, ";
            line += $"{Length.Value}, ";
            return line;
        }

        internal void SetData(List<string> values)
        {
            values.RemoveAt(0); //index
            AngleConstraint.Value = GetVectorFromData(values[0]);
            values.RemoveAt(0); //angle constraint
            RotationMultiplier.Value = GetVectorFromData(values[0]);
            values.RemoveAt(0); //rotation multiplier
            TailMassMultiplier.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //tail mass multiplier
            Length.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //length
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

