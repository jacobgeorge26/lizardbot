using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        public static int SuccessorCache { get; set; } = 50;
    }
}
