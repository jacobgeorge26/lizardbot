using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class BaseConfig : object
    {
        public static bool IsDefault { get; set; } = true;

        public static int NoSections { get; set; } = -1;

        public static int DefaultNoSections { get; set; } = 3;

        public static List<GameObject> Sections { get; set; } = new List<GameObject>();

        public static List<BodyConfig> SectionConfigs { get; set; } = new List<BodyConfig>();

        public static bool IsTailEnabled { get; set; } = true;
        public static GameObject Tail { get; set; }
    }
}


