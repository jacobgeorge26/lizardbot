using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class RobotConfig : object
    {
        public int RobotIndex;

        public GameObject Object;

        public int Version = 0;

        public bool IsEnabled = false;

        public float Performance = 0;

        public int MutationCount = 0;

        public RobotConfig Original;

        public Gene NoSections = new Gene(5, 1, 10, Variable.NoSections);

        public Gene IsTailEnabled = new Gene(true, Variable.IsTailEnabled);

        //lower the better
        public Gene BodyColour = new Gene(150, 0, 255, Variable.BodyColour);

        //when changes are made, should serpentine motion be preserved
        public Gene MaintainSerpentine = new Gene(true, Variable.MaintainSerpentine);

        public RobotConfig(int _index, GameObject _object)
        {
            this.RobotIndex = _index;
            this.Object = _object;
        }

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();

        //not directly a copy, a clean copy without the default / random init
        //excludes object, this should have been setup with the constructor
        public void FreshCopy(RobotConfig robot, int version)
        {
            this.RobotIndex = robot.RobotIndex;
            this.Version = version;
            this.IsEnabled = false;
            this.Performance = 0;
            this.MutationCount = robot.MutationCount;
            this.Original = robot.Original;
            this.NoSections.Value = robot.NoSections.Value;
            this.IsTailEnabled.Value = robot.IsTailEnabled.Value;
            this.BodyColour.Value = robot.BodyColour.Value;
            this.MaintainSerpentine.Value = robot.MaintainSerpentine.Value;
        }

    }
}


