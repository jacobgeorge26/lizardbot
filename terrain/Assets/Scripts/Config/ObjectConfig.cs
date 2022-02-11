using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class ObjectConfig : MonoBehaviour
    {
        [HideInInspector]
        public BodyPart Type;

        [HideInInspector]
        public int Index;

        [HideInInspector]
        public int RobotIndex;

        public void Init(int _index, BodyPart _type, int _robotIndex)
        {
            Index = _index;
            Type = _type;
            RobotIndex = _robotIndex;
        }

    }
}
