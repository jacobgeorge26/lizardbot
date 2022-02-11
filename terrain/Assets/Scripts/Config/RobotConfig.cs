using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class RobotConfig : MonoBehaviour
    {
        public int RobotIndex;

        public int Version = 0;

        public bool IsEnabled = false;

        public float Performance = 0;

        public int MutationCount = 0;

        public RangedVariable NoSections = new RangedVariable(5, 1, 10, Variable.Physical);

        public BaseVariable IsTailEnabled = new BaseVariable(true, Variable.Physical);

        public RangedVariable BodyColour = new RangedVariable(150, 0, 255, Variable.Physical);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();
    }
}


