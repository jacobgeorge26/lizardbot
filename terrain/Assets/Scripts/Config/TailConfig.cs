using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class TailConfig : JointConfig
    {
        public GeneVariable TailMassMultiplier = new GeneVariable(0.5f, 0.2f, 1.5f, Variable.TailMassMultiplier);

        public void Clone(TailConfig oldConfig)
        {
            AngleConstraint.Value = oldConfig.AngleConstraint.Value;
            RotationMultiplier.Value = oldConfig.RotationMultiplier.Value;
            TailMassMultiplier.Value = oldConfig.TailMassMultiplier.Value;
        }
    }
}

