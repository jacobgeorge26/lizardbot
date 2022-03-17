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
        StartCoroutine(GenerateRobots());
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
        if(AIConfig.LogPerformanceData || AIConfig.Debugging) StartCoroutine(LogPerformance());
    }


    public IEnumerator LogPerformance()
    {
        //debugging overrides AIConfig.LogData - only shows data on grapher
        bool firstLineWritten = false;
        if (AIConfig.LogPerformanceData) SetupPerformanceWriter();
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {
                if (AIConfig.LogPerformanceData)
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
                if (AIConfig.Debugging)
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
