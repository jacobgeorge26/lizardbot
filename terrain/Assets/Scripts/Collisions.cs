using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Terrain")
        {
            FindObjectOfType<SuccessorFunction>().Enable();
        }
    }
}
