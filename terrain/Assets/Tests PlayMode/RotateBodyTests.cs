using System;
using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RotateBodyTests
{
    private GameObject robot;
    private GameObject section;
    private Rigidbody body;
    private MoveBody sectionMS;
    private BodyConfig sectionBC;
    private GameObject env;

    [SetUp]
    public void Init()
    {
        robot = new GameObject();
        RobotConfig robotConfig = new RobotConfig(0, robot);
        robotConfig.RobotIndex = 1;
        AIConfig.RobotConfigs.Add(robotConfig);

        //setup all objects that the tests will use
        env = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));

        section = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        sectionMS = section.GetComponent<MoveBody>();
        sectionBC = new BodyConfig();
        ObjectConfig HObjConfig = section.GetComponent<ObjectConfig>();
        HObjConfig.Init(0, BodyPart.Body, sectionBC, 1);
        body = section.GetComponent<Rigidbody>();
        body.useGravity = false;

        sectionBC.AngleConstraint.Value = new Vector3(0, 60, 0);
        sectionBC.IsDriving.Value = false;
        sectionBC.RotationMultiplier.Value = new Vector3(1, 1, 1);
        sectionBC.DriveVelocity.Value = 1f;

        
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(robot);
        GameObject.Destroy(section);
        GameObject.Destroy(env);
    }

    //if rotating clockwise it should increase the angle
    [UnityTest]
    public IEnumerator Y_RotatesClockwiseOnInit()
    {
        body.rotation = Quaternion.Euler(0, 0, 0);
        sectionBC.IsRotating.Value = true;

        yield return new WaitForSeconds(3);
        Assert.IsTrue(sectionMS.GetRelativeAngle().y > 0);
    }


    //if AngleConstraint is 0 then rotate should hover around zero
    [UnityTest]
    public IEnumerator Y_ZeroMaxAngle()
    {
        body.rotation = Quaternion.Euler(0, 0, 0);
        body.velocity = new Vector3();
        sectionBC.IsRotating.Value = true;
        sectionBC.AngleConstraint.Value = new Vector3();

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForSeconds(5); 
        Assert.IsTrue(Math.Abs(sectionMS.GetRelativeAngle().y) < 5);
    }

    //if RotationMultiplier is 0 then rotate should have no effect
    [UnityTest]
    public IEnumerator Y_ZeroTurnRatio()
    {
        body.rotation = Quaternion.Euler(0, 0, 0);
        body.velocity = new Vector3();
        sectionBC.IsRotating.Value = true;
        sectionBC.RotationMultiplier.Value = new Vector3();

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(0, sectionMS.GetRelativeAngle().y);
    }

    //if IsRotating is false then a frame update should have no effect
    [UnityTest]
    public IEnumerator Y_NotRotating()
    {
        body.rotation = Quaternion.Euler(0, 0, 0);
        body.velocity = new Vector3();
        sectionBC.IsRotating.Value = false;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(0, sectionMS.GetRelativeAngle().y);
    }

}
