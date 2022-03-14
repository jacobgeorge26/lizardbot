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
            yield return new WaitForSeconds(0.5f);
        }
        //enable UI
        ui.Enable();
        StartCoroutine(LogPerformance());
    }


    public IEnumerator LogPerformance()
    {
        int attempt = PlayerPrefs.GetInt("Attempt") + 1;
        PlayerPrefs.SetInt("Attempt", attempt);
        string filePath = "../terrain/Report/Data/PerformanceLogs.csv";
        writer = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        writer.WriteLine($"ATTEMPT NO {attempt}");
        writer.WriteLine("Time, Mean Performance");
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {
                //calculate average
                float average = AIConfig.RobotConfigs.Average(r => r.Performance);
                writer.WriteLine($"{Time.realtimeSinceStartup.ToString()}, {average}");
            }
        }
    }

    void OnApplicationQuit()
    {
        writer.Close();
    }
}
