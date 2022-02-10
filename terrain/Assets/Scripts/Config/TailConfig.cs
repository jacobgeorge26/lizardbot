using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class TailConfig : JointConfig
    {
        public RangedVariable TailMassMultiplier = new RangedVariable(0.1f, 0.1f, 1.5f);
    }
}

