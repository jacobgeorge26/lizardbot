using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraPosTests : MonoBehaviour
{
    private GameObject robot;
    private GameObject env;
    private GameObject cam;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        robot = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot"));
        cam = robot.transform.GetChild(5).gameObject;

        env = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(robot);
        GameObject.Destroy(env);
    }

    //check that the camera is following the 
    [UnityTest]
    public IEnumerator CameraPosition()
    {
        GameObject head = robot.transform.GetChild(0).gameObject;
        head.transform.position = new Vector3(5, 10, 20);

        GameObject tail = robot.transform.GetChild(4).gameObject;
        tail.transform.position = new Vector3(2, 15, 20);

        yield return null;
        Vector3 expected = new Vector3(3.5f, 17.5f, 17.5f);

        Assert.AreEqual(cam.transform.position, expected);
    }
}
