using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraPosTests : MonoBehaviour
{
    private GameObject head, tail;
    private GameObject env;
    private GameObject cam;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        env = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));

        head = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        MoveBody headScript = head.GetComponent<MoveBody>();
        headScript.IsDriving = false;
        headScript.IsRotating = false;

        tail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        MoveBody tailScript = tail.GetComponent<MoveBody>();
        tailScript.IsDriving = false;
        tailScript.IsRotating = false;

        cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        CameraPosition camScript = cam.GetComponent<CameraPosition>();
        camScript.Head = head;
        camScript.Tail = tail;    
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(head);
        GameObject.Destroy(tail);
        GameObject.Destroy(cam);
        GameObject.Destroy(env);
        Debug.Log("test");
    }

    //check that the camera is following the 
    [UnityTest]
    public IEnumerator CameraPosition()
    {
        head.transform.position = new Vector3(5, 10, 20);
        tail.transform.position = new Vector3(2, 15, 20);

        yield return null;
        Vector3 expected = new Vector3(3.5f, 17.5f, 17.5f);

        Assert.AreEqual(cam.transform.position, expected);
    }
}
