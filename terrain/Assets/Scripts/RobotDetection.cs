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
        //if there is another robot in the area
        if(potential.Length > expectedColliders)
        {
            List<GameObject> incoming = new List<GameObject>();
            //list of possible layers, remove current and any layers being used by those in the area
            List<int> layers = Enumerable.Range(6, 25).ToList();
            layers.Remove(gameObject.layer);
            //get all objects in the area
            foreach (var item in potential)
            {
                //only interested in the heads - as these are tagged
                if(item.gameObject.tag == "Robot" && item.gameObject.GetComponent<ObjectConfig>().RobotIndex != robotConfig.RobotIndex)
                {
                    //there is another robot in the area
                    if (item.gameObject.layer == gameObject.layer)
                    {
                        //layer needs to be changed
                        incoming.Add(item.gameObject);
                        layers.Remove(item.gameObject.layer);
                    }
                }
            }
            //if there are no layers available then alert that there is a chance the robots will interact
            if(incoming.Count > 0 && layers.Count == 0)
            {
                Debug.LogWarning("There are no available layers in a busy area, there is a chance that there will be interaction between the robots.");
            }
            else if(incoming.Count > 0)
            {
                //there are layers available, replace the layer of this robot (all objects) with a randomly selected new layer
                int oldLayer = gameObject.layer;
                int newLayer = layers[Random.Range(0, layers.Count)];
                gameObject.layer = newLayer;
                foreach (Transform child in gameObject.transform.parent.transform)
                {
                    GameObject childObject = child.gameObject;
                    childObject.layer = newLayer;
                }
                //also update the sphere attached to the head to enable collision detection without impacting the behaviour of the robot
                foreach (Transform child in gameObject.transform)
                {
                    if (child.GetComponent<Rigidbody>() != null)
                    {
                        //this is the sphere we're looking for
                        child.gameObject.layer = newLayer;
                    }
                }
                //Debug.Log($"Moving {gameObject.name} from layer {LayerMask.LayerToName(oldLayer)} to layer {LayerMask.LayerToName(newLayer)}");
            }
        }

    }
}
