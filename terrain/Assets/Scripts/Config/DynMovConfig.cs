using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class DynMovConfig : object
    {
        public static bool UseDynamicMovement = true;

        //how many points around the robot should be analysed in the dynamic movement
        public static int NoSphereSamples = 50;

        public static Vector3[] SpherePoints = new Vector3[NoSphereSamples];

        //how frequently should the movement be analysed - i.e. how many seconds should the movement be left for
        public static float SampleRate = 2f;

        //how frequently should the movement be adjusted
        public static float AdjustRate = 5f;

        //how large an angle is allowed when selecting suitable points
        //recommend at least 10
        public static int AdjustSensitivity = 10;

        public static float ActivationRate = 0.3f;
    }
}
