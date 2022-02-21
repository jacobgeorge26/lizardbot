using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraPosTests : MonoBehaviour
{
    private GameObject robot, head, tail;
    private GameObject cam;
    private CameraPosition camScript;

    [SetUp]
    public void Init()
    {
        robot = new GameObject();
        RobotConfig robotConfig = robot.AddComponent<RobotConfig>();
        robotConfig.NoSections.Value = 1;

        head = new GameObject();
        ObjectConfig headConfig = head.AddComponent<ObjectConfig>();
        headConfig.Init(0, BodyPart.Body, 0);
        robotConfig.Configs.Add(headConfig);

        tail = new GameObject();
        ObjectConfig tailConfig = tail.AddComponent<ObjectConfig>();
        headConfig.Init(0, BodyPart.Tail, 0);
        robotConfig.Configs.Add(tailConfig);

        cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        CameraConfig.RobotCamera = cam;
        CameraConfig.Hat = new GameObject();
        camScript = cam.GetComponent<CameraPosition>();
        camScript.SetRobot(robotConfig);
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(robot);
        GameObject.Destroy(head);
        GameObject.Destroy(tail);
        GameObject.Destroy(cam);
        GameObject.Destroy(CameraConfig.Hat);
    }

    //check that the camera is following the 
    [UnityTest]
    public IEnumerator CameraPosition()
    {
        head.transform.position = new Vector3(5, 10, 20);
        tail.transform.position = new Vector3(2, 15, 20);

        yield return null;
        Vector3 expected = new Vector3(3.5f, 17.5f, 17.5f);

        Assert.AreEqual(expected, camScript.GetCameraPosition());
    }
}
