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
        sectionMS.InitSetup();
        sectionBC = section.GetComponent<BodyConfig>();

        section2 = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        section2_MS = section2.GetComponent<MoveBody>();
        section2_MS.InitSetup();
        section2_BC = section2.GetComponent<BodyConfig>();
    }

    //GetAngle should return an unrounded vector as is (transform.localEulerAngles)
    [Test]
    public void GetAngleTest()
    {
        Vector3 angle = new Vector3(1.5f, 2.5f, 3.5f);
        Quaternion rotation = Quaternion.Euler(angle);
        sectionMS.GetComponent<Rigidbody>().rotation = rotation;
        Assert.IsTrue(sectionMS.GetAngle() == angle);        
    }

    //if params ask for a rounded version of GetAngle then this should be returned
    [Test]
    public void GetAngleTest_Rounded()
    {
        Quaternion rotation = Quaternion.Euler(1.5f, 2.5f, 3.5f);
        sectionMS.GetComponent<Rigidbody>().rotation = rotation;
        Assert.AreEqual(sectionMS.GetAngle(true), new Vector3(2f, 3f, 4f));
    }

    //if angle is >180 then it should be adjusted (-360) to get it into range -180 < x <= 0
    [Test]
    public void GetRelativeAngleTest_PositiveRangeAdjustment()
    {
        Quaternion rotation = Quaternion.Euler(60, 200, 180);
        sectionMS.GetComponent<Rigidbody>().rotation = rotation;
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(60, -160, 180));
    }

    //if angle is <=-180 then it should be adjusted (+360) to get it into range 0 <= x <= 180
    [Test]
    public void GetRelativeAngleTest_NegativeRangeAdjustment()
    {
        Quaternion rotation = Quaternion.Euler(-60, -200, -180);
        sectionMS.GetComponent<Rigidbody>().rotation = rotation;
        Assert.AreEqual(sectionMS.GetRelativeAngle(), new Vector3(-60, 160, 180));
    }

    //default of GetRelativeAngle is rounded, if params ask for unrounded then this should be returned
    [Test]
    public void GetRelativeAngleTest_NotRounded()
    {
        Vector3 angle = new Vector3(1.5f, 2.5f, 3.5f);
        Quaternion rotation = Quaternion.Euler(angle);
        sectionMS.GetComponent<Rigidbody>().rotation = rotation;
        Assert.IsTrue(sectionMS.GetAngle() == angle);
    }
}
