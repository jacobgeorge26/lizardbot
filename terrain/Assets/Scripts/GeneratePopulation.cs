using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class GeneratePopulation : MonoBehaviour
{
    private StreamWriter performanceWriter;
    public void CreatePopulation()
    {
        DynMovConfig.SpherePoints = GetSpherePoints();
        StartCoroutine(GenerateRobots());
    }

    //This method was taken from this stackoverflow post
    //https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere
    private Vector3[] GetSpherePoints()
    {
        int radius = 10;
        Vector3[] points = new Vector3[DynMovConfig.NoSphereSamples];
        var phi = Mathf.PI * (3 - Mathf.Sqrt(5));
        for (int i = 0; i < DynMovConfig.NoSphereSamples; i++)
        {
            var y = 1 - (i / (DynMovConfig.NoSphereSamples - 1f)) * 2;
            var r = Mathf.Sqrt(1 - y * y);
            var theta = phi * i;
            var x = Mathf.Cos(theta) * r;
            var z = Mathf.Sin(theta) * r;

            points[i] = new Vector3(x, y, z) * radius;
        }
        return points;
    }

    IEnumerator GenerateRobots()
    {
        //setup camera
        CameraConfig.Hat = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Hat"));
        CameraConfig.Hat.name = "hat";
        CameraConfig.Hat.SetActive(false);
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "Robot Camera";
        cam.SetActive(false);
        CameraConfig.RobotCamera = cam;
        //setup UI
        UIDisplay ui = UIConfig.UIContainer.gameObject.GetComponent<UIDisplay>();
        if (ui != null && UIConfig.IsUIEnabled) ui.enabled = true;
        else UIConfig.UIContainer.SetActive(false);
        //generate population, leaving a gap between them
        for (int i = 0; i < AIConfig.PopulationSize; i++)
        {
            GameObject robot = new GameObject();
            robot.name = $"Robot {i + 1}";
            GameObject version = new GameObject();
            version.transform.parent = robot.transform;
            version.AddComponent<GenerateRobot>();
            yield return new WaitForSeconds(0.2f);
        }
        //enable UI
        if(UIConfig.IsUIEnabled) ui.Enable();
        if(DebugConfig.LogPerformanceData || DebugConfig.IsDebugging) StartCoroutine(LogPerformance());
    }

    internal IEnumerator RespawnRobot(RobotConfig oldRobot)
    {
        yield return new WaitForSeconds(2f);
        GameObject version = new GameObject();
        version.transform.parent = oldRobot.Object.transform.parent;
        version.name = oldRobot.Object.name;
        version.AddComponent<GenerateRobot>();
    }


    public IEnumerator LogPerformance()
    {
        //debugging overrides AIConfig.LogData - only shows data on grapher
        bool firstLineWritten = false;
        if (DebugConfig.LogPerformanceData) SetupPerformanceWriter();
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {
                if (DebugConfig.LogPerformanceData)
                {
                    string line = "Time, ";
                    if (!firstLineWritten)
                    {
                        firstLineWritten = true;
                        AIConfig.RobotConfigs.ForEach(r => line += $"Robot {r.RobotIndex + 1}, ");
                        performanceWriter.WriteLine(line);
                    }
                    line = $"{Time.realtimeSinceStartup.ToString()}";
                    AIConfig.RobotConfigs.ForEach(r => line += $"{r.Performance}, ");
                    performanceWriter.WriteLine(line);
                }

                //debugging
                if (DebugConfig.IsDebugging)
                {
                    List<RobotConfig> ordered = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).ToList();
                    float max = ordered.Take(AIConfig.PopulationSize / 10).Average(r => r.Performance);
                    Grapher.Log(max, "Top 10%", Color.red);
                    max = ordered.Take(AIConfig.PopulationSize / 4).Average(r => r.Performance);
                    Grapher.Log(max, "Top 25%", Color.black);
                }
            }
        }
    }

    private void SetupPerformanceWriter()
    {
        string filePath = "../terrain/Report/Data/PerformanceLogs.csv";
        performanceWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        performanceWriter.WriteLine($"ATTEMPT NO {PlayerPrefs.GetInt("Attempt")}");
        performanceWriter.WriteLine("Time, Mean Performance");
    }

    void OnDestroy()
    {
        if (performanceWriter != null) performanceWriter.Close();
    }


}
