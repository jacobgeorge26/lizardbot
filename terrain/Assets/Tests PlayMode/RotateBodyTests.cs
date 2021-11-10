using System;
using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RotateBodyTests
{
    private GameObject section;
    private MoveBody sectionMS;
    private BodyConfig sectionBC;
    private GameObject env;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        env = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));

        section = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        sectionMS = section.GetComponent<MoveBody>();
        sectionBC = section.GetComponent<BodyConfig>();

        sectionBC.MaxAngle = new int [3]{ 0, 60, 0 };
        sectionBC.IsDriving = false;
        sectionBC.IsClockwise = new bool[3] { false, true, false };
        sectionBC.TurnVelocity = 180;
        sectionBC.DriveVelocity = 1f;

        
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(section);
        GameObject.Destroy(sectionMS);
        GameObject.Destroy(sectionBC);
        GameObject.Destroy(env);
    }

    //if rotating clockwise it should increase the angle
    [UnityTest]
    public IEnumerator Y_Positive()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = true;
        sectionBC.IsClockwise = new bool[3]{false, true, false};

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y > 0);
    }

    //if rotating anticlockwise it should decrease the angle
    [UnityTest]
    public IEnumerator Y_Negative()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);       
        sectionBC.IsRotating = true;
        sectionBC.IsClockwise = new bool[3] { false, false, false };

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y < 0);
    }

    //check that Rotate is reversing direction at MaxAngle
    [UnityTest]
    public IEnumerator Y_PositiveReverseDirection()
    {
        section.transform.localEulerAngles = new Vector3(0, 60, 0);
        sectionBC.IsRotating = true;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y < 60);
    }

    //check that Rotate is reversing direction at MaxAngle * -1
    [UnityTest]
    public IEnumerator Y_NegativeReverseDirection()
    {
        section.transform.localEulerAngles = new Vector3(0, -60, 0);
        sectionBC.IsRotating = true;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y > -60);
    }

    //check that IsClockwise will reverse if bug and angle > MaxAngle
    [UnityTest]
    public IEnumerator Y_PositiveHandleMaxAngleExceeded()
    {
        section.transform.localEulerAngles = new Vector3(0, 80, 0);
        sectionBC.IsRotating = true;
        sectionBC.IsClockwise = new bool[3] { false, true, false };

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsFalse(sectionBC.IsClockwise[1]);
    }

    //check that IsClockwise will reverse if bug and angle < MaxAngle * -1
    [UnityTest]
    public IEnumerator Y_NegativeHandleMaxAngleExceeded()
    {
        section.transform.localEulerAngles = new Vector3(0, -80, 0);
        sectionBC.IsRotating = true;
        sectionBC.IsClockwise = new bool[3] { false, false, false };

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionBC.IsClockwise[1]);
    }

    //if MaxAngle is 0 then rotate should hover around zero
    [UnityTest]
    public IEnumerator Y_ZeroMaxAngle()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = true;
        sectionBC.MaxAngle = new int[3] { 0, 0, 0 };

        yield return new WaitForSeconds(2); 
        Assert.IsTrue(Math.Abs(sectionMS.GetRelativeAngle().y) < 5);
    }

    //if TurnVelocity is 0 then rotate should have no effect
    [UnityTest]
    public IEnumerator Y_ZeroTurnVelocity()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = true;
        sectionBC.TurnVelocity = 0;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(sectionMS.GetRelativeAngle().y, 0);
    }

    //if IsRotating is false then a frame update should have no effect
    [UnityTest]
    public IEnumerator Y_NotRotating()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = false;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(0, 0, 0));
    }

}
