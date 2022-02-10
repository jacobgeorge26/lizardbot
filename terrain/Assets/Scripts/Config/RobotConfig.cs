using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class RobotConfig : MonoBehaviour
    {
        public int RobotIndex;

        public int Version = 0;

        public bool IsEnabled = false;

        public float Performance = 0;

        public GameObject LastRobot = null;

        public RangedVariable NoSections = new RangedVariable(5, 1, 10, Variable.Physical);

        public BaseVariable IsTailEnabled = new BaseVariable(true, Variable.Physical);

        public List<ObjectConfig> Configs { get; set; } = new List<ObjectConfig>();

        //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
        public float GetYPos()
        {
            return TerrainConfig.GetTerrainHeight() + 1f;
        }

        //accessed by GenerateRobot on init and GeneticAlgorithm when respawning
        public float GetZPos(GameObject prevObject, GameObject thisObject)
        {
            if (prevObject == null) return 0;
            //determine the position of this section by the location of the prev section, and size of both
            float zPos = prevObject.transform.position.z;
            zPos += -1 * (prevObject.transform.localScale.z / 2 + thisObject.transform.localScale.z / 2 + 0.1f);
            return zPos;
        }
    }
}


