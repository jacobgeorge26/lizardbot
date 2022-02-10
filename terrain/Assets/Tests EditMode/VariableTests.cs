using Config;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class VariableTests
{
    [Test]
    public void BaseVariableTest_bool()
    {
        bool value = false;
        BaseVariable variable = new BaseVariable(value, Variable.Physical);
        variable.Value = !value;
        Assert.AreEqual(!value, variable.Value);
    }

    [Test]
    public void BaseVariableTest_int()
    {
        int value = 3;
        BaseVariable variable = new BaseVariable(value, Variable.Physical);
        variable.Value -= 1;
        Assert.AreEqual(value - 1, variable.Value);
    }

    [Test]
    public void BaseVariableTest_float()
    {
        float value = 3;
        BaseVariable variable = new BaseVariable(value, Variable.Physical);
        variable.Value -= 1.5f;
        Assert.AreEqual(value - 1.5f, variable.Value);
    }

    [Test]
    public void BaseVariableTest_vector3()
    {
        Vector3 value = new Vector3(30, 60, 45);
        BaseVariable variable = new BaseVariable(value, Variable.Physical);
        Assert.AreEqual(variable.Value, value);
    }

    [Test]
    public void BaseVariableTest_NonCompatible()
    {
        string value = "hello";
        Assert.Throws<System.Exception>(() => new BaseVariable(value, Variable.Physical));
    }

    [Test]
    public void RangedVariableTest_int()
    {
        int value = 3;
        RangedVariable variable = new RangedVariable(value, 0, 5, Variable.Physical);
        variable.Value -= 1;
        Assert.AreEqual(value - 1, variable.Value);
    }

    [Test]
    public void RangedVariableTest_int_BelowMin()
    {
        int value = 1, min = 2;
        RangedVariable variable = new RangedVariable(value, min, 5, Variable.Physical);
        Assert.AreEqual(min, variable.Value);
    }

    [Test]
    public void RangedVariableTest_int_AboveMax()
    {
        int value = 5, max = 4;
        RangedVariable variable = new RangedVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(max, variable.Value);
    }

    [Test]
    public void RangedVariableTest_vector3()
    {
        Vector3 value = new Vector3(30, 60, 45);
        RangedVariable variable = new RangedVariable(value, 10, 90, Variable.Physical);
        variable.Value = value / 2;
        Assert.AreEqual(new Vector3(15, 30, 22.5f), variable.Value);
    }

    [Test]
    public void RangedVariableTest_vector3_BelowMin()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float min = 45;
        RangedVariable variable = new RangedVariable(value, min, 90, Variable.Physical);
        Assert.AreEqual(new Vector3(45, 60, 45), variable.Value);
    }

    [Test]
    public void RangedVariableTest_vector3_AboveMax()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float max = 45;
        RangedVariable variable = new RangedVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(new Vector3(30, 45, 45), variable.Value);
    }

    [Test]
    public void RangedVariableTest_MinAboveMax()
    {
        int value = 5, min = 20, max = 4;
        Assert.Throws<System.Exception>(() => new RangedVariable(value, min, max, Variable.Physical));
    }

    [Test]
    public void RangedVariableTest_NonCompatible()
    {
        bool value = true;
        Assert.Throws<System.Exception>(() => new RangedVariable(value, 0, 5, Variable.Physical));
    }
}
