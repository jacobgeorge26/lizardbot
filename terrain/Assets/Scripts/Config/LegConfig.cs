using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class LegConfig : JointConfig
    {
        public int AttachedBody;

        public Vector3 SpawnPoint;

        public Gene Length = new Gene(2f, 1f, 4f, Variable.Length);

        public Gene Mass = new Gene(1f, 0.5f, 1.5f, Variable.Mass);

        public LegConfig(int _attachedBody, Vector3 _spawnPoint)
        {
            AttachedBody = _attachedBody;
            SpawnPoint = _spawnPoint;
        }

        public void Clone(LegConfig oldConfig)
        {
            //TODO: LEGS - complete this
        }
    }
}

