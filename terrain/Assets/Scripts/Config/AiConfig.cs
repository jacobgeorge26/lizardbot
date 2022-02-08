using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        public static List<RobotConfig> RobotConfigs = new List<RobotConfig>();

        //25 is a hard limit as it is based on the number of layers
        public static RangedVariable PopulationSize = new RangedVariable(2, 1, 25);
    }
}
