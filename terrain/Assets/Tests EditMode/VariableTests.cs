using Config;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class VariableTests
{
    [SetUp]
    public void Init()
    {
        AIConfig.RandomInitValues = false;
    }

    [Test]
    public void standard_int()
    {
        int value = 3;
        GeneVariable variable = new GeneVariable(value, 0, 5, Variable.Physical);
        variable.Value -= 1;
        Assert.AreEqual(value - 1, variable.Value);
    }

    [Test]
    public void int_BelowMin()
    {
        int value = 1, min = 2;
        GeneVariable variable = new GeneVariable(value, min, 5, Variable.Physical);
        Assert.AreEqual(min, variable.Value);
    }

    [Test]
    public void int_AboveMax()
    {
        int value = 5, max = 4;
        GeneVariable variable = new GeneVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(max, variable.Value);
    }

    [Test]
    public void vector3()
    {
        Vector3 value = new Vector3(30, 60, 45);
        GeneVariable variable = new GeneVariable(value, 10, 90, Variable.Physical);
        variable.Value = value / 2;
        Assert.AreEqual(new Vector3(15, 30, 22.5f), variable.Value);
    }

    [Test]
    public void vector3_BelowMin()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float min = 45;
        GeneVariable variable = new GeneVariable(value, min, 90, Variable.Physical);
        Assert.AreEqual(new Vector3(45, 60, 45), variable.Value);
    }

    [Test]
    public void vector3_AboveMax()
    {
        Vector3 value = new Vector3(30, 60, 45);
        float max = 45;
        GeneVariable variable = new GeneVariable(value, 0, max, Variable.Physical);
        Assert.AreEqual(new Vector3(30, 45, 45), variable.Value);
    }

    [Test]
    public void MinAboveMax()
    {
        int value = 5, min = 20, max = 4;
        Assert.Throws<System.Exception>(() => new GeneVariable(value, min, max, Variable.Physical));
    }

    [Test]
    public void NonCompatible()
    {
        double value = 0.5;
        Assert.Throws<System.Exception>(() => new GeneVariable(value, 0, 5, Variable.Physical));
    }

    [Test]
    public void bool_CorrectInit()
    {
        GeneVariable variable = new GeneVariable(true, Variable.Physical);
        Assert.AreEqual(true, variable.Value);
    }

    [Test]
    public void bool_IncorrectInit()
    {
        GeneVariable variable = new GeneVariable(true, 0, 1, Variable.Physical);
        Assert.AreEqual(true, variable.Value);
    }


    //slightly unstable as it could randomly generate the input value
    //don't test with bool as this is 50/50 going to return same result due to bool handling
    [Test]
    public void RandomInit()
    {
        AIConfig.RandomInitValues = true;
        GeneVariable variable = new GeneVariable(17, 1, 100, Variable.Physical);
        Assert.AreNotEqual(17, variable.Value);
    }
}
