using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Config
{
    public class LegConfig : JointConfig
    {
        public int AttachedBody;

        public LegPosition Position;

        public Gene Length = new Gene(2f, 1f, 2f, Variable.Length);

        public Gene Mass = new Gene(1f, 0.5f, 1.5f, Variable.Mass);

        //known issue in math when offset is > 50 - hard max at 50 until I have time to revisit it
        public Gene AngleOffset = new Gene(20, 0, 50, Variable.AngleOffset);

        public Gene GaitMultiplier = new Gene(1.5f, 1f, 2f, Variable.GaitMultiplier);

        public LegConfig(int _attachedBody, int _position)
        {
            AttachedBody = _attachedBody;
            Position = (LegPosition)_position;
        }

        public void Clone(LegConfig oldConfig)
        {
            AngleConstraint.Value = oldConfig.AngleConstraint.Value;
            RotationMultiplier.Value = oldConfig.RotationMultiplier.Value;
            Length.Value = oldConfig.Length.Value;
            Mass.Value = oldConfig.Mass.Value;
            AngleOffset.Value = oldConfig.AngleOffset.Value;
        }

        internal string GetHeader()
        {
            string line = "";
            line += $"Leg No, ";
            line += $"{nameof(AttachedBody)}, ";
            line += $"{nameof(Position)}, ";
            line += $"{nameof(AngleConstraint)}, ";
            line += $"{nameof(RotationMultiplier)}, ";
            line += $"{nameof(Length)}, ";
            line += $"{nameof(Mass)}, ";
            line += $"{nameof(AngleOffset)}, ";
            line += $"{nameof(GaitMultiplier)}, ";
            return line;
        }

        internal string GetData(int index)
        {
            string line = "";
            line += $"{index}, ";
            line += $"{AttachedBody}, ";
            line += $"{Position}, ";
            line += $"{Regex.Replace(AngleConstraint.Value.ToString(), @",\s", " - ")}, ";
            line += $"{Regex.Replace(RotationMultiplier.Value.ToString(), @",\s", " - ")}, ";
            line += $"{Length.Value}, ";
            line += $"{Mass.Value}, ";
            line += $"{AngleOffset.Value}, ";
            line += $"{GaitMultiplier.Value}, ";
            return line;
        }

        internal void SetData(List<string> values)
        {
            values.RemoveAt(0); //index
            values.RemoveAt(0); //attached body
            values.RemoveAt(0); //position
            AngleConstraint.Value = GetVectorFromData(values[0]);
            values.RemoveAt(0); //angle constraint
            RotationMultiplier.Value = GetVectorFromData(values[0]);
            values.RemoveAt(0); //rotation multiplier
            Length.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //length
            Mass.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //mass
            AngleOffset.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //angle offset
            GaitMultiplier.Value = (float)Convert.ToDouble(values[0]);
            values.RemoveAt(0); //gait multiplier
        }

        internal void SetEmptyData(List<string> values)
        {
            values.RemoveAt(0); //index
            values.RemoveAt(0); //attached body
            values.RemoveAt(0); //position
            values.RemoveAt(0); //angle constraint
            values.RemoveAt(0); //rotation multiplier
            values.RemoveAt(0); //length
            values.RemoveAt(0); //mass
            values.RemoveAt(0); //angle offset
            values.RemoveAt(0); //gait multiplier
        }

        internal string GetEmptyData(int index)
        {
            string line = "";
            line += $"{index}, ";
            line += $"{AttachedBody}, ";
            line += $"{Position}, ";
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

