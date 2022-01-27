using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class RobotConfig : MonoBehaviour
    {
        public int RobotIndex;

        public RangedVariable NoSections = new RangedVariable(5, 1, 10);

        public BaseVariable IsTailEnabled = new BaseVariable(true);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();
    }
}


