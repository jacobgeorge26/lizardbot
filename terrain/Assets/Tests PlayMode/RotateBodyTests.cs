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

        sectionBC.AngleConstraint = new int [3]{ 0, 60, 0 };
        sectionBC.IsDriving = false;
        sectionBC.RotationMultiplier = new float[3] { 1, 1, 1};
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
    public IEnumerator Y_RotatesClockwiseOnInit()
    {
        section.GetComponent<Rigidbody>().rotation = Quaternion.Euler(0, 0, 0);
        section.GetComponent<Rigidbody>().velocity = new Vector3();
        sectionBC.IsRotating = true;

        yield return new WaitForSeconds(3);
        Assert.IsTrue(sectionMS.GetRelativeAngle().y > 0);
    }


    //check that Rotate is reversing direction at AngleConstraint
    [UnityTest]
    public IEnumerator Y_PositiveReverseDirection()
    {
        section.transform.localEulerAngles = new Vector3(0, 60, 0);
        sectionBC.IsRotating = true;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y < 60);
    }

    //check that Rotate is reversing direction at AngleConstraint * -1
    [UnityTest]
    public IEnumerator Y_NegativeReverseDirection()
    {
        section.transform.localEulerAngles = new Vector3(0, -60, 0);
        sectionBC.IsRotating = true;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.GetRelativeAngle().y > -60);
    }


    //if AngleConstraint is 0 then rotate should hover around zero
    [UnityTest]
    public IEnumerator Y_ZeroMaxAngle()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = true;
        sectionBC.AngleConstraint = new int[3] { 0, 0, 0 };

        yield return new WaitForSeconds(2); 
        Assert.IsTrue(Math.Abs(sectionMS.GetRelativeAngle().y) < 5);
    }

    //if RotationMultiplier is 0 then rotate should have no effect
    [UnityTest]
    public IEnumerator Y_ZeroTurnRatio()
    {
        section.transform.localEulerAngles = new Vector3(0, 0, 0);
        sectionBC.IsRotating = true;
        sectionBC.RotationMultiplier = new float[3] { 0, 0, 0 };

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
