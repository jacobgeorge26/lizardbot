using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField]
    private GameObject head;   
    public GameObject Head
    {
        get { return head; }
        set { head = value; }
    }

    [SerializeField]
    private GameObject tail;
    public GameObject Tail
    {
        get { return tail; }
        set { tail = value; }
    }

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
        this.transform.position = new Vector3((headInfo.x + tailInfo.x)/2, (headInfo.y + tailInfo.y)/2 + offset, (headInfo.z + tailInfo.z)/2 - offset/2);
    }
}
