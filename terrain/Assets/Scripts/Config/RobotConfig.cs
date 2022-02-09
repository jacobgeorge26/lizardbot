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

        public RangedVariable NoSections = new RangedVariable(5, 1, 10);

        public BaseVariable IsTailEnabled = new BaseVariable(true);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();

        public void Copy(RobotConfig robot)
        {
            this.RobotIndex = robot.RobotIndex;
            this.Version = robot.Version + 1;
            this.NoSections = robot.NoSections;
            this.IsTailEnabled = robot.IsTailEnabled;
            this.Configs = robot.Configs;
        }
    }
}


