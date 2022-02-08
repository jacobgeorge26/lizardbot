using Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotDetection : MonoBehaviour
{
    private int expectedColliders;
    private RobotConfig robotConfig;
    // Start is called before the first frame update
    void Start()
    {
        //find the expected number of colliders, to know if another robot is approaching
        int robotIndex = GetComponent<ObjectConfig>().RobotIndex;
        robotConfig = AIConfig.RobotConfigs[robotIndex];

        expectedColliders = robotConfig.NoSections.Value; //one for each section of the body
        expectedColliders++; //terrain
        expectedColliders++; //sphere contained in head for collision detection
        expectedColliders += robotConfig.IsTailEnabled.Value ? 1 : 0; //tail
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] potential = Physics.OverlapSphere(this.transform.position, 10);
        if(potential.Length > expectedColliders)
        {
            List<GameObject> incoming = new List<GameObject>();
            //shoddy but faster than other methods and these are static values due to number of layers available
            List<int> layers = Enumerable.Range(6, 25).ToList();
            layers.Remove(gameObject.layer);
            foreach (var item in potential)
            {
                if(item.gameObject.tag == "Robot" && item.gameObject.GetComponent<ObjectConfig>().RobotIndex != robotConfig.RobotIndex)
                {
                    //there is another robot in the area
                    incoming.Add(item.gameObject);
                    layers.Remove(item.gameObject.layer);
                }
            }
            Debug.Log(incoming.Count);
        }

    }
}
