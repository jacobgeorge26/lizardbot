using Config;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class VariableTests
{
    [Test]
    public void GeneVariableTest_int()
    {
        int value = 3;
        GeneVariable variable = new GeneVariable(value, 0, 5, Variable.Physical);
        variable.Value -= 1;
        Assert.AreEqual(value - 1, variable.Value);
    }

    [Test]
    public void GeneVariableTest_int_BelowMin()
    {
        int value = 1, min = 2;
        GeneVariable variable = new GeneVariable(value, min, 5, Variable.Physical);
        Assert.AreEqual(min, variable.Value);
    }

    [Test]
    public void GeneVariableTest_int_AboveMax()
    {
        int value = 5, max = 4;
        GeneVariable variable = new GeneVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(max, variable.Value);
    }

    [Test]
    public void GeneVariableTest_vector3()
    {
        Vector3 value = new Vector3(30, 60, 45);
        GeneVariable variable = new GeneVariable(value, 10, 90, Variable.Physical);
        variable.Value = value / 2;
        Assert.AreEqual(new Vector3(15, 30, 22.5f), variable.Value);
    }

    [Test]
    public void GeneVariableTest_vector3_BelowMin()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float min = 45;
        GeneVariable variable = new GeneVariable(value, min, 90, Variable.Physical);
        Assert.AreEqual(new Vector3(45, 60, 45), variable.Value);
    }

    [Test]
    public void GeneVariableTest_vector3_AboveMax()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float max = 45;
        GeneVariable variable = new GeneVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(new Vector3(30, 45, 45), variable.Value);
    }

    [Test]
    public void GeneVariableTest_MinAboveMax()
    {
        int value = 5, min = 20, max = 4;
        Assert.Throws<System.Exception>(() => new GeneVariable(value, min, max, Variable.Physical));
    }

    [Test]
    public void GeneVariableTest_NonCompatible()
    {
        double value = 0.5;
        Assert.Throws<System.Exception>(() => new GeneVariable(value, 0, 5, Variable.Physical));
    }

    [Test]
    public void GeneVariableTest_Bool_CorrectInit()
    {
        GeneVariable variable = new GeneVariable(true, Variable.Physical);
        Assert.AreEqual(true, variable.Value);
    }

    [Test]
    public void GeneVariableTest_Bool_IncorrectInit()
    {
        GeneVariable variable = new GeneVariable(true, 0, 1, Variable.Physical);
        Assert.AreEqual(true, variable.Value);
    }
}
