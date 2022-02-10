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
        public GameObject Object;

        [HideInInspector]
        public int RobotIndex;

        public void Init(int _index, BodyPart _type, GameObject _object, int _robotIndex)
        {
            Index = _index;
            Type = _type;
            Object = _object;
            RobotIndex = _robotIndex;
        }

    }
}
