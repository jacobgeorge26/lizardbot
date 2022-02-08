using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        public static List<RobotConfig> RobotConfigs = new List<RobotConfig>();

        public static RangedVariable PopulationSize = new RangedVariable(2, 1, 10);
    }
}
