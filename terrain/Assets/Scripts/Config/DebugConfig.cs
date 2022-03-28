using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class DebugConfig : object
    {
        //error handling system - not strictly debugging

        //used by error handling to recreate the population
        public static List<RobotConfig> InitRobots = new List<RobotConfig>();

        public static bool IsTotalRespawn = false;


        //visualise stuck points

        //used to show points where a robot got stuck
        public static GameObject StuckPoints;

        //should the stuck points be shown
        public static bool ShowStuckPoints = false;


        //data collection

        //recommend one or the other for LogPerformanceData and Debugging
        //if true then data will be extracted to csv files in the Report folder
        public static bool LogPerformanceData = false;

        public static bool LogRobotData = false;

        public static bool LogAIData = true;

        public static RobotConfig BestRobot;

        //grapher

        //if true then data will be shown in grapher
        public static bool IsDebugging = false;

        public static string GetHeader()
        {
            string line = "";
            line += $"{nameof(AIConfig.PopulationSize)}, ";
            line += $"{nameof(AIConfig.RandomInitValues)}, ";
            line += $"{nameof(AIConfig.Sensitivity)}, ";
            line += $"{nameof(AIConfig.MutationCycle)}, ";
            line += $"{nameof(AIConfig.RecombinationRate)}, ";
            line += $"{nameof(AIConfig.RecombinationType)}, ";
            line += $"{nameof(AIConfig.MutationRate)}, ";
            line += $"{nameof(AIConfig.MutationType)}, ";
            line += $"{nameof(AIConfig.SelectionSize)}, ";
            return line;
        }

        public static string GetData()
        {
            string line = "";
            line += $"{AIConfig.PopulationSize}, ";
            line += $"{AIConfig.RandomInitValues}, ";
            line += $"{AIConfig.Sensitivity}, ";
            line += $"{AIConfig.MutationCycle}, ";
            line += $"{AIConfig.RecombinationRate}, ";
            line += $"{AIConfig.RecombinationType}, ";
            line += $"{AIConfig.MutationRate}, ";
            line += $"{AIConfig.MutationType}, ";
            line += $"{AIConfig.SelectionSize}, ";
            return line;
        }
    }
}