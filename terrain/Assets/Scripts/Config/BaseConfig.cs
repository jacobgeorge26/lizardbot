using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class BaseConfig : object
    {
        public static bool IsDefault { get; set; } = true;

        public static RangedVariable NoSections = new RangedVariable(5, 1, 10);

        public static List<GameObject> Sections { get; set; } = new List<GameObject>();

        public static List<BodyConfig> SectionConfigs { get; set; } = new List<BodyConfig>();

        public static BaseVariable IsTailEnabled = new BaseVariable(true);
        public static GameObject Tail { get; set; }

        public static RangedVariable TailMassMultiplier = new RangedVariable(0.1f, 0.1f, 1.5f);
    }
}


