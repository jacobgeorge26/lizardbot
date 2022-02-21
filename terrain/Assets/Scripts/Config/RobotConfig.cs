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

        public GeneVariable NoSections = new GeneVariable(5, 1, 10, Variable.NoSections);

        public GeneVariable IsTailEnabled = new GeneVariable(true, Variable.IsTailEnabled);

        public GeneVariable BodyColour = new GeneVariable(150, 0, 255, Variable.BodyColour);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();

        //not directly a copy, a clean copy without the default / random init
        public void FreshCopy(RobotConfig robot, int version)
        {
            this.RobotIndex = robot.RobotIndex;
            this.Version = version;
            this.IsEnabled = false;
            this.Performance = 0;
            this.MutationCount = 0;
            this.Original = robot.Original;
            this.NoSections = robot.NoSections;
            this.IsTailEnabled = robot.IsTailEnabled;
            this.BodyColour = robot.BodyColour;
        }
    }
}


