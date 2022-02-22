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

        [HideInInspector]
        public BodyConfig Body;

        [HideInInspector]
        public TailConfig Tail;

        public void Init(int _index, BodyPart _type, JointConfig _config, int _robotIndex)
        {
            Index = _index;
            Type = _type;
            Body = Type == BodyPart.Body ? (BodyConfig)_config : null;
            Tail = Type == BodyPart.Tail ? (TailConfig)_config : null;
            RobotIndex = _robotIndex;
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }

    }
}
