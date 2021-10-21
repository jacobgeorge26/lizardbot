using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MoveJointTests
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public void GetAngleTest()
    {
        //GameObject robot =
        //    MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot"));
        //Joint game = Transform.FindObjectOfType<>
        //Vector3 before = game.GetAngle();
        //yield return null;
        //Vector3 after = game.GetAngle();
        Assert.AreNotEqual(1, 2);
    }
}
