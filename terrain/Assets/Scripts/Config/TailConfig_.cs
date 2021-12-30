using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class TailConfig : object
    {
        public static Vector3 PositionOffset = new Vector3(0, 0, 0);

        private static Vector3 _angleConstraint = new Vector3(60, 60, 60);

        private static Vector3 _rotationMultiplier = new Vector3(0.1f, 0.2f, 0.1f);

        public static JointConfig JointConfig = new JointConfig(_angleConstraint, _rotationMultiplier);
    }
}

