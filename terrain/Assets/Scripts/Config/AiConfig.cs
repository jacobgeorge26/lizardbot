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
        public static RangedVariable PopulationSize = new RangedVariable(10, 1, 100);
    }
}
