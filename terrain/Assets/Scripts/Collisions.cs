using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    private bool trappedAlgorithmTriggered = false;
    void OnTriggerEnter(Collider collider)
    {
        if(!trappedAlgorithmTriggered && collider.tag == "Terrain")
        {
            trappedAlgorithmTriggered = true;
            FindObjectOfType<TrappedAlgorithm>().Enable();
        }
    }
}
