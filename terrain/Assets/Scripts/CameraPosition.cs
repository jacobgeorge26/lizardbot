using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField]
    public GameObject Head;

    [SerializeField]
    public GameObject Tail;


    private float offset = 5;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = GetCameraPosition();
    }

    public Vector3 GetCameraPosition()
    {
        Vector3 newPos = new Vector3();
        newPos.x = (Head.transform.position.x + Tail.transform.position.x) / 2;
        newPos.y = (Head.transform.position.y + Tail.transform.position.y) / 2 + offset;
        newPos.z = (Head.transform.position.z + Tail.transform.position.z) / 2 - (offset / 2);
        return newPos;
    }
}
