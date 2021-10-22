using System.Collections;
using Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraPosTests : MonoBehaviour
{
    private GameObject section;
    private MoveBody sectionMS;
    private BodyConfig sectionBC;

    [SetUp]
    public void Init()
    {
        //setup all objects that the tests will use
        section = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Section"));
        sectionMS = section.GetComponent<MoveBody>();
        sectionBC = section.GetComponent<BodyConfig>();

        MonoBehaviour.Instantiate(Resources.Load<GameObject>("BaseEnv"));
    }

    [TearDown]
    public void CleanUp()
    {
        //destroy all objects used
        GameObject.Destroy(section);
        GameObject.Destroy(sectionMS);
        GameObject.Destroy(sectionBC);
    }
}
