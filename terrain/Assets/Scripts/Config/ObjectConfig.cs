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

        public void Remove()
        {
            Destroy(this.gameObject);
        }


        //this method is used to delete an entire robot - uses the first objectconfig it is storing (there must always be >0)
        //calls this to delete it, though ObjectConfig isn't directly related to the Robot
        //messy, but avoids messier code when RobotConfig was a MonoBehaviour too
        public void Remove(GameObject robot)
        {
            Destroy(robot);
        }

        //this method is used to clone an entire robot - uses the first objectconfig it is storing (there must always be >0)
        //calls this to clone it, though ObjectConfig isn't directly related to the Robot
        //messy, but avoids messier code when RobotConfig was a MonoBehaviour too
        public GameObject Clone(GameObject robot)
        {
            GameObject newRobot = Instantiate(robot);
            return newRobot;
        }

    }
}
