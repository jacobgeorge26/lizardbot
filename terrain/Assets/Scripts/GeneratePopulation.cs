using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GeneratePopulation : MonoBehaviour
{
    private StreamWriter writer;
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
        UIDisplay ui = FindObjectOfType<UIDisplay>();
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
                        writer.WriteLine(line);
                    }
                    line = $"{Time.realtimeSinceStartup.ToString()}";
                    AIConfig.RobotConfigs.ForEach(r => line += $"{r.Performance}, ");
                    writer.WriteLine(line);
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
        int attempt = PlayerPrefs.GetInt("Attempt") + 1;
        PlayerPrefs.SetInt("Attempt", attempt);
        string filePath = "../terrain/Report/Data/PerformanceLogs.csv";
        writer = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        writer.WriteLine($"ATTEMPT NO {attempt}");
        writer.WriteLine("Time, Mean Performance");
    }

    private void SetupRobotWriter()
    {
        string filePath = "../terrain/Report/Data/BestRobots.csv";
        writer = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        writer.WriteLine($"ATTEMPT NO {PlayerPrefs.GetInt("Attempt")}");
        string header = "";
        header += AIConfig.GetHeader();
        header += AIConfig.BestRobot.GetHeader();
        writer.WriteLine(header);
    }

    void OnApplicationQuit()
    {
        if (writer != null) writer.Close();
        if (AIConfig.LogRobotData && AIConfig.BestRobot != null)
        {
            SetupRobotWriter();
            string data = "";
            data += AIConfig.GetData();
            data += AIConfig.BestRobot.GetData();
            writer.WriteLine(data);
            writer.Close();
        }
    }


}
