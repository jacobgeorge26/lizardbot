using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class EntireBodyConfig : MonoBehaviour
    {
        //Objects
        [Header("Game Objects")]

        [SerializeField]
        protected GameObject Head;
        [SerializeField]
        protected List<GameObject> Joints;
        [SerializeField]
        protected List<GameObject> Sections;

        //TO DO: convert to JointSetup where:
        [Header("Joint Setup")]
        [SerializeField]
        [Range(5, 60)]
        protected double maxAngle;

        [SerializeField]
        protected bool[] Locked;

        [SerializeField]
        protected bool[] Direction; //true = clockwise rotation when coiling


        // Start is called before the first frame update
        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {

        }
    }



}
