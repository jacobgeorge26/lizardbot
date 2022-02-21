using Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotDetection : MonoBehaviour
{
    private RobotConfig robotConfig;
    private RobotHelpers helpers;
    // Start is called before the first frame update
    void Start()
    {
        //find the expected number of colliders, to know if another robot is approaching
        int robotIndex = GetComponent<ObjectConfig>().RobotIndex;
        robotConfig = AIConfig.RobotConfigs[robotIndex];
        helpers = robotConfig.gameObject.GetComponent<RobotHelpers>();
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> incoming = helpers.GetNearbyRobots(10);
        //list of possible layers, remove current and any layers being used by those in the area
        List<int> layers = Enumerable.Range(6, 25).ToList();
        layers.Remove(gameObject.layer);
        incoming.RemoveAll(r => r.layer != gameObject.layer);
        incoming.ForEach(r => layers.Remove(r.gameObject.layer));
        //if there are no layers available then alert that there is a chance the robots will interact
        if (incoming.Count > 0 && layers.Count == 0)
        {
            Debug.LogWarning("There are no available layers in a busy area, there is a chance that there will be interaction between the robots.");
        }
        else if (incoming.Count > 0)
        {
            //there are layers available, replace the layer of this robot (all objects) with a randomly selected new layer
            int newLayer = layers[Random.Range(0, layers.Count)];
            gameObject.layer = newLayer;
            //set robot object's layer - no collisions but easier to have them all in same layer
            GameObject robotObject = gameObject.transform.parent.gameObject;
            robotObject.layer = newLayer;
            robotObject.GetComponent<RobotHelpers>().SetChildLayer(newLayer);
        }

    }
}
