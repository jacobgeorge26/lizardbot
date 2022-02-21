using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class TailConfig : JointConfig
    {
        public GeneVariable TailMassMultiplier = new GeneVariable(0.5f, 0.2f, 1.5f, Variable.TailMassMultiplier);
    }
}

