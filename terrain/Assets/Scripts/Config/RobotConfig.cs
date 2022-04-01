using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    public class RobotConfig : object
    {
        public int RobotIndex;

        public GameObject Object;

        public int Version = 0;

        public bool IsEnabled = false;

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();

        public float Performance = 0;

        public int MutationCount = 0;

        //how far did the robot move for each vector index
        public float[] Distances = new float[DynMovConfig.NoSphereSamples];

        public float StartTime = 0;

        public int PenaltyCount = 0;

        //GENES

        public RobotConfig Original;

        public Gene NoSections = new Gene(5, 1, 10, Variable.NoSections);

        public Gene NoLegs = new Gene(4, 1, 20, Variable.NoLegs);

        public Gene IsTailEnabled = new Gene(false, Variable.IsTailEnabled);

        //lower the better
        public Gene BodyColour = new Gene(150, 0, 255, Variable.BodyColour);

        //when changes are made, should serpentine motion be preserved
        public Gene MaintainSerpentine = new Gene(true, Variable.MaintainSerpentine);

        //should the legs maintain a lizardlike gait
        public Gene MaintainGait = new Gene(true, Variable.MaintainGait);

        //should the size & mass be maintained across the body
        public Gene UniformBody = new Gene(false, Variable.UniformBody);

        public RobotConfig(int _index, GameObject _object)
        {
            this.RobotIndex = _index;
            this.Object = _object;
            this.StartTime = Time.time;
        }

        //RESPAWN
        //clone variables applicable to starting again from a scrapped attempt - different to a fresh copy
        public void Clone(RobotConfig oldRobot)
        {
            RobotIndex = oldRobot.RobotIndex;
            Version = oldRobot.Version;
            Original = oldRobot.Original;
            MutationCount = oldRobot.MutationCount;
            StartTime = oldRobot.StartTime;
            PenaltyCount = oldRobot.PenaltyCount;
            NoSections.Value = oldRobot.NoSections.Value;
            NoLegs.Value = oldRobot.NoLegs.Value;
            IsTailEnabled.Value = oldRobot.IsTailEnabled.Real;
            BodyColour.Value = oldRobot.BodyColour.Value;
            MaintainSerpentine.Value = oldRobot.MaintainSerpentine.Real;
            UniformBody.Value = oldRobot.UniformBody.Real;
        }

        //NEW
        //not directly a copy, a clean copy without the default / random init
        //excludes object, this should have been setup with the constructor
        public void FreshCopy(RobotConfig robot, int version)
        {
            this.RobotIndex = robot.RobotIndex;
            this.Version = version;
            this.IsEnabled = false;
            this.Performance = 0;
            this.MutationCount = robot.MutationCount;
            this.StartTime = Time.time;
            this.PenaltyCount = 0;
            this.Original = robot.Original;
            this.NoSections.Value = robot.NoSections.Value;
            this.NoLegs.Value = robot.NoLegs.Value;
            this.IsTailEnabled.Value = robot.IsTailEnabled.Value;
            this.BodyColour.Value = robot.BodyColour.Value;
            this.MaintainSerpentine.Value = robot.MaintainSerpentine.Value;
            this.UniformBody.Value = robot.UniformBody.Value;
        }

        public string GetHeader()
        {
            string line = "";
            line += $"{nameof(Version)}, ";
            line += $"{nameof(Performance)}, ";
            line += $"Terrain, ";
            line += $"{nameof(RobotIndex)}, ";
            line += $"{nameof(NoSections)}, ";
            line += $"{nameof(NoLegs)}, ";
            line += $"{nameof(IsTailEnabled)}, ";
            line += $"{nameof(BodyColour)}, ";
            line += $"{nameof(MaintainSerpentine)}, ";
            line += $"{nameof(UniformBody)}, ";
            BodyConfig tempBody = new BodyConfig();
            for (int i = 0; i < NoSections.Max; i++)
            {
                line += tempBody.GetHeader();
            }
            line += new TailConfig().GetHeader();
            return line;
        }


        public string GetData()
        {
            string line = "";
            line += $"{Version}, ";
            line += $"{Performance}, ";
            line += $"{TerrainConfig.GetTerrainType(RobotIndex)}, ";
            line += $"{RobotIndex}, ";
            line += $"{NoSections.Value}, ";
            line += $"{NoLegs.Value}, ";
            line += $"{IsTailEnabled.Value}, ";
            line += $"{BodyColour.Value}, ";
            line += $"{MaintainSerpentine.Value}, ";
            line += $"{UniformBody.Value}, ";
            BodyConfig tempBody = new BodyConfig();
            for (int i = 0; i < NoSections.Max; i++)
            {
                var body = Configs.Where(o => o.Type == BodyPart.Body && o.Index == i).ToList();
                if (body.Count != 0) line += body.First().Body.GetData(i);
                else line += tempBody.GetEmptyData(i);
            }
            var tail = Configs.Where(o => o.Type == BodyPart.Tail).ToList();
            if (tail.Count != 0) line += tail.First().Tail.GetData();
            else line += new TailConfig().GetEmptyData();
            return line;
        }

    }
}


