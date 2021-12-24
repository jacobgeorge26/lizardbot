using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class AIConfig : object
    {
        private static float _searchLength = 1f;

        static float searchMin = 0.5f, searchMax = 3f;
        public static float SearchLength
        {
            get => _searchLength;
            set
            {
                if (value < searchMin)
                    _searchLength = searchMin;
                else if (value > searchMax)
                    _searchLength = searchMax;
                else
                    _searchLength = value;
            }
        }
    }
}
