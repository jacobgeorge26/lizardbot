using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class JointConfig : MonoBehaviour
    {
        public bool IsLocked { get; set; } = false;
        public Vector3 RelativeAngle { get; set; }
        public int MaxAngle { get; set; } = 45;

        public int CoilAngle { get; set; } = 10;

        public GameObject PreviousSection { get; set; }

        public GameObject NextSection { get; set; }

    }
}

