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

        public RobotConfig Original;

        public GeneVariable NoSections = new GeneVariable(5, 1, 10, Variable.Physical);

        public GeneVariable IsTailEnabled = new GeneVariable(true, Variable.Physical);

        public GeneVariable BodyColour = new GeneVariable(150, 0, 255, Variable.Physical);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();
    }
}


