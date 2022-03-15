using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        public static List<RobotConfig> RobotConfigs = new List<RobotConfig>();

        //there is a hard limit of 25 layers available
        //RobotDetection works to prevent robots in the same area being in the same layer
        //population can be >25, but in rougher terrain where the robot isn't making progress, expect some warnings and interaction between them
        public static int PopulationSize = 50;

        public static RobotConfig[] LastRobots = new RobotConfig[PopulationSize];

        public static Vector3[] SpawnPoints = new Vector3[Mathf.CeilToInt(PopulationSize / 25f)];

        //should the robots be randomly generated
        //false uses the preset defaults
        public static bool RandomInitValues = true;

        //how many locations are analysed (2 per second)
        public static int Sensitivity = 20;

        //how many generations of mutations are allowed to pass before it determines the success of the robot
        //set to 0 to always select the mutated robot
        public static int MutationCycle = 2;

        //the recombination rate accounts for whether the robot will recombine this generation
        //for no recombination set recombination rate to 0
        //2 decimal places recommmended
        public static float RecombinationRate = 0.2f;

        //list of the possible recombinations that will be used
        //the recombination rate accounts for whether the robot will recombine this generation
        public static Recombination RecombinationType = Recombination.Triad;

        //the mutation rate accounts for whether the robot will mutate this generation
        //for no mutation set mutation rate to 0
        //2 decimal places recommmended
        public static float MutationRate = 0.3f;

        //options for which genes will be mutated
        public static Mutation MutationType = Mutation.Both;

        //value for k - how many should be considered in the recombination?
        public static int SelectionSize = 5;

        //DEBUGGING
        //used to show points where a robot got stuck
        public static GameObject StuckPoints;

        //should the stuck points be shown
        public static bool ShowStuckPoints = false;

        //recommend one or the other for LogData and Debugging
        //if true then data will be extracted to csv files in the Report folder
        public static bool LogPerformanceData = false;

        public static bool LogRobotData = true;

        public static RobotConfig BestRobot;

        //if true then data will be shown in grapher
        public static bool Debugging = false;

        public static string GetHeader()
        {
            string line = "";
            line += $"{nameof(PopulationSize)}, ";
            line += $"{nameof(RandomInitValues)}, ";
            line += $"{nameof(Sensitivity)}, ";
            line += $"{nameof(MutationCycle)}, ";
            line += $"{nameof(RecombinationRate)}, ";
            line += $"{nameof(RecombinationType)}, ";
            line += $"{nameof(MutationRate)}, ";
            line += $"{nameof(MutationType)}, ";
            line += $"{nameof(SelectionSize)}, ";
            return line;
        }

        public static string GetData()
        {
            string line = "";
            line += $"{PopulationSize}, ";
            line += $"{RandomInitValues}, ";
            line += $"{Sensitivity}, ";
            line += $"{MutationCycle}, ";
            line += $"{RecombinationRate}, ";
            line += $"{RecombinationType}, ";
            line += $"{MutationRate}, ";
            line += $"{MutationType}, ";
            line += $"{SelectionSize}, ";
            return line;
        }
    }
}
