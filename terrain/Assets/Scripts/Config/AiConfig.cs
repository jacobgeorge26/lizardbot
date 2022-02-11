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
        public static int PopulationSize = 3;

        public static Selection Selection = Selection.Performance;

        //the recombination rate accounts for whether the robot will recombine this generation
        //for no recombination set recombination rate to 0
        //2 decimal places recommmended
        public static float RecombinationRate = 0.25f;

        //list of the possible recombinations that will be used
        //the recombination rate accounts for whether the robot will recombine this generation
        public static Recombination RecombinationType = Recombination.PhysicalLikeness;

        //the mutation rate accounts for whether the robot will mutate this generation
        //for no mutation set mutation rate to 0
        //2 decimal places recommmended
        public static float MutationRate = 0.4f;

        //options for which genes will be mutated
        public static Mutation MutationType = Mutation.Any;
    }
}
