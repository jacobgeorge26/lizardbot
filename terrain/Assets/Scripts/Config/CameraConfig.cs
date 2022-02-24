using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class CameraConfig : object
    {
        //MUST be between 0 <= x < PopulationSize
        public static int CamFollow = -1;

        public static GameObject RobotCamera;

        public static GameObject OverviewCamera;

        public static GameObject Hat;
    }
}