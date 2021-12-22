using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class BaseConfig : object
    {
        public static bool IsDefault { get; set; } = true;

        public static int NoSections { get; set; } = -1;

        public static int DefaultNoSections { get; set; } = 2;

        public static List<GameObject> Sections { get; set; } = new List<GameObject>();

        public static List<BodyConfig> SectionConfigs { get; set; } = new List<BodyConfig>();
    }
}


