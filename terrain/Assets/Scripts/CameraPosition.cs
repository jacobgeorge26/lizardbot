using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject head;

    [SerializeField]
    private GameObject tail;

    private float offset = 5;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 headInfo = head.transform.position;
        Vector3 tailInfo = tail.transform.position;
        cam.transform.position = new Vector3((headInfo.x + tailInfo.x)/2, (headInfo.y + tailInfo.y)/2 + offset, (headInfo.z + tailInfo.z)/2 - offset/2);
    }
}
