using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MoveJointTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void GetAngleTest()
    {
       
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        GameObject robot =
        MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Robot"));
        yield return null;
    }
}
