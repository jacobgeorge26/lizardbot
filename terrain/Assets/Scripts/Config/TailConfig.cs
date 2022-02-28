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
    }
}

