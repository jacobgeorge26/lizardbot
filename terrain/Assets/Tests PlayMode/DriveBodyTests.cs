using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DriveBodyTests : MonoBehaviour
{
    private GameObject section;
    private MoveBody sectionMS;
    private BodyConfig sectionBC;
    private GameObject env;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        section = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        sectionMS = section.GetComponent<MoveBody>();
        sectionBC = section.GetComponent<BodyConfig>();

        env = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));
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

    //if driving then should move forward
    [UnityTest]
    public IEnumerator Drive()
    {
        section.transform.position = new Vector3(0, 0, 0);
        section.transform.rotation = Quaternion.Euler(0, 0, 0);
        sectionBC.IsDriving = true;
        sectionBC.IsRotating = false;
        sectionBC.DriveVelocity.Value = 1f;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(sectionMS.transform.position.z > 0);
    }

    //if DriveVelocity is zero then should have no effect
    public IEnumerator ZeroDriveVelocity()
    {
        section.transform.position = new Vector3(0, 0, 0);
        section.transform.rotation = Quaternion.Euler(0, 0, 0);
        sectionBC.IsDriving = true;
        sectionBC.IsRotating = false;
        sectionBC.DriveVelocity.Value = 0;

        yield return new WaitForFixedUpdate(); //first one it won't move
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(sectionMS.transform.position.z, 0);
    }
}
