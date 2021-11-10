using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MoveBodyHelperMethodTests
{
    private GameObject section, section2;
    private MoveBody sectionMS, section2_MS;
    private BodyConfig sectionBC, section2_BC;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        section = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        sectionMS = section.GetComponent<MoveBody>();
        sectionBC = section.GetComponent<BodyConfig>();

        section2 = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        section2_MS = section2.GetComponent<MoveBody>();
        section2_BC = section2.GetComponent<BodyConfig>();
    }

    //GetAngle should return an unrounded vector as is (transform.localEulerAngles)
    [Test]
    public void GetAngleTest()
    {
        Vector3 angle = new Vector3(1.5f, 2.5f, 3.5f);
        section.transform.localEulerAngles = angle;
        Assert.IsTrue(sectionMS.GetAngle() == angle);        
    }

    //if params ask for a rounded version of GetAngle then this should be returned
    [Test]
    public void GetAngleTest_Rounded()
    {
        sectionMS.transform.localEulerAngles = new Vector3(1.5f, 2.5f, 3.5f);
        Assert.AreEqual(sectionMS.GetAngle(true), new Vector3(2f, 3f, 4f));
    }

    //GetRelativeAngle should return angle relative to parent and adjusted (see further tests)
    [Test]
    public void GetRelativeAngleTest()
    {
        section.transform.localEulerAngles = new Vector3(0, 30, 0);

        section2.transform.localEulerAngles = new Vector3(0, -30, 0);
        section2.transform.parent = section.transform;

        section2_BC.IsRotating = true;

        Assert.AreEqual(section2_MS.GetRelativeAngle(), new Vector3(0, -60, 0));
    }

    //if section is 'locked' then it's relative rotation should be 0 to suggest no change from parent
    [Test]
    public void GetRelativeAngleTest_NotRotating()
    {
        section.transform.localEulerAngles = new Vector3(1.5f, 2.5f, 3.5f);
        sectionBC.IsRotating = false;
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(0, 0, 0));
    }

    //if angle is >180 then it should be adjusted (-360) to get it into range -180 < x <= 0
    [Test]
    public void GetRelativeAngleTest_PositiveRangeAdjustment()
    {
        section.transform.localEulerAngles = new Vector3(60, 200, 180);
        sectionBC.IsRotating = true;
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(60, -160, 180));
    }

    //if angle is <=-180 then it should be adjusted (+360) to get it into range 0 <= x <= 180
    [Test]
    public void GetRelativeAngleTest_NegativeRangeAdjustment()
    {
        section.transform.localEulerAngles = new Vector3(-60, -200, -180);
        sectionBC.IsRotating = true;
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(-60, 160, 180));
    }

    //default of GetRelativeAngle is rounded, if params ask for unrounded then this should be returned
    [Test]
    public void GetRelativeAngleTest_NotRounded()
    {
        Vector3 angle = new Vector3(1.5f, 2.5f, 3.5f);
        section.transform.localEulerAngles = angle;
        sectionBC.IsRotating = true;
        Assert.IsTrue(sectionMS.GetAngle() == angle);
    }
}
