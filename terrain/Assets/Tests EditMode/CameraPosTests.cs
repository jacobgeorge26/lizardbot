using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraPosTests : MonoBehaviour
{
    private GameObject head = new GameObject();
    private GameObject tail = new GameObject();
    private GameObject cam;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        CameraPosition camScript = cam.GetComponent<CameraPosition>();
        camScript.Head = head;
        camScript.Tail = tail;    
    }

    //check that the camera is following the 
    [Test]
    public void CameraPosition()
    {
        head.transform.position = new Vector3(5, 10, 20);
        tail.transform.position = new Vector3(2, 15, 20);

        Vector3 expected = new Vector3(3.5f, 17.5f, 17.5f);

        Assert.AreEqual(cam.GetComponent<CameraPosition>().GetCameraPosition(), expected);
    }
}
