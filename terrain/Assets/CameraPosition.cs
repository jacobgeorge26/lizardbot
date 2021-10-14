using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject robot;

    private float offset = 5;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 robotInfo = robot.transform.position;
        cam.transform.position = new Vector3(0, robotInfo.y + offset, robotInfo.z);
    }
}
