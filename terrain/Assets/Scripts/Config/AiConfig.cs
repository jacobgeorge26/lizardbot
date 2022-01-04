using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        public static RangedVariable SearchLength = new RangedVariable(1f, 0.5f, 3f);

        public static int SampleSize = 50;
    }
}
