using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{

    public static GameController Controller;

    StreamWriter aiPerformanceWriter, aiWriter, robotWriter;
    GeneratePopulation generate;
    int attemptCount;
    float startTime = 0, pauseTime = 0, elapsedTime = 0;

    void Start()
    {
        ////////////////////////
        PlayerPrefs.SetInt("Attempt", 50);

        Controller = this;
        attemptCount = AIConfig.NoAttempts;
        //setup writers with headers
        string filePath = "../terrain/Report/Data/AIPerformance.csv";
        aiPerformanceWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        aiPerformanceWriter.WriteLine($"ATTEMPT, Time, Performance");
        filePath = "../terrain/Report/Data/AIConfig.csv";
        aiWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        aiWriter.WriteLine($"ATTEMPT, {AIConfig.GetHeader()}");
        StartCoroutine(GenerateAttempt());
    }

    private IEnumerator GenerateAttempt()
    {
        if (AIConfig.PopulationSize > 0)
        {         
            for (int i = attemptCount; i > 0; i--)
            {
                //IMPORTANT delay needed to allow unity to stop running scripts attached to newly disabled objects
                yield return new WaitForSeconds(3f);
                attemptCount--;
                UpdateAttempt(1);
                if(pauseTime == 0) SetupAIParams();
                generate = gameObject.AddComponent<GeneratePopulation>();
                Debug.Log($"ATTEMPT {PlayerPrefs.GetInt("Attempt")}");
                generate.CreatePopulation();
                pauseTime = pauseTime == 0 ? 0 : Time.realtimeSinceStartup; //update pause time after respawn
                StartCoroutine(SaveAttemptData());
                yield return new WaitForSeconds(AIConfig.AttemptLength - elapsedTime);
                pauseTime = 0;
                elapsedTime = 0;
                AIConfig.RobotConfigs.ForEach(r => {
                    r.Object.transform.parent.gameObject.SetActive(false);
                });
                if (AIConfig.LogRobotData) LogBestRobot();
                StartCoroutine(Deconstruct(false));
            }
        }
    }

    private void SetupAIParams()
    {
        AIConfig.MutationCycle = (int)Random.Range(0, 10);
        AIConfig.RecombinationRate = Random.value;
        AIConfig.MutationRate = Random.value;
        AIConfig.SelectionSize = Random.Range(1, 20);
        int attempt = PlayerPrefs.GetInt("Attempt");
        aiWriter.WriteLine($"{attempt}, {AIConfig.GetData()}");
    }

    private IEnumerator SaveAttemptData()
    {
        int attempt = PlayerPrefs.GetInt("Attempt");
        startTime = pauseTime == 0 ? startTime : Time.realtimeSinceStartup;
        while (PlayerPrefs.GetInt("Attempt") == attempt)
        {
            yield return new WaitForSeconds(10f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {               
                List<RobotConfig> ordered = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).ToList();
                int take = Mathf.CeilToInt(AIConfig.PopulationSize / 4f);
                float performance = ordered.Take(take).Average(r => r.Performance);
                float time = pauseTime == 0 ? Time.realtimeSinceStartup - startTime : Time.realtimeSinceStartup - pauseTime + elapsedTime;
                aiPerformanceWriter.WriteLine($"{attempt}, {time}, {performance}");
            }
        }

    }

    private void UpdateAttempt(int add)
    {
        int attempt = PlayerPrefs.GetInt("Attempt") + add;
        PlayerPrefs.SetInt("Attempt", attempt);
    }

    private void LogBestRobot()
    {
        if (AIConfig.BestRobot == null && AIConfig.RobotConfigs.Count > 0) AIConfig.BestRobot = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).First();
        if (AIConfig.BestRobot != null)
        {
            if (robotWriter == null) SetupRobotWriter();
            string data = $"{ PlayerPrefs.GetInt("Attempt")}, ";
            data += AIConfig.GetData();
            data += AIConfig.BestRobot.GetData();
            robotWriter.WriteLine(data);
        }
    }

    private StreamWriter SetupRobotWriter()
    {
        string filePath = "../terrain/Report/Data/BestRobots.csv";
        robotWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        robotWriter.WriteLine();
        string header = "ATTEMPT, ";
        header += AIConfig.GetHeader();
        header += AIConfig.BestRobot.GetHeader();
        robotWriter.WriteLine(header);
        return robotWriter;
    }

    internal void TotalRespawn(string exception)
    {
        //stop all execution - an error has occurred
        AIConfig.RobotConfigs.ForEach(r => {
            r.Object.transform.parent.gameObject.SetActive(false);
        });
        Debug.LogWarning($"Respawning attempt {PlayerPrefs.GetInt("Attempt")}. \n {exception}");
        StopAllCoroutines();
        pauseTime = Time.realtimeSinceStartup;
        elapsedTime += pauseTime - startTime;
        AIConfig.InitRobots = AIConfig.RobotConfigs;
        StartCoroutine(Deconstruct(true));
        //get ready to restart attempt
        attemptCount++;
        UpdateAttempt(-1);
        StartCoroutine(GenerateAttempt());
    }

    internal void SingleRespawn(string exception, RobotConfig robot)
    {
    }

    private IEnumerator Deconstruct(bool delay)
    {
        Destroy(CameraConfig.RobotCamera);
        Destroy(CameraConfig.Hat);
        Destroy(generate);
        AIConfig.RobotConfigs.ForEach(r => {
            r.Configs.ForEach(o => {
                if(AIConfig.InitRobots == null)
                {
                    Destroy(r.Object.transform.parent.gameObject);
                    o.Body = null;
                    o.Tail = null;
                }
            });
        });
        AIConfig.RobotConfigs = new List<RobotConfig>();
        AIConfig.BestRobot = null;
        AIConfig.LastRobots = new RobotConfig[AIConfig.PopulationSize];
        if(AIConfig.InitRobots != null)
        {
            yield return new WaitUntil(() => AIConfig.InitRobots.Count == AIConfig.RobotConfigs.Count);
            AIConfig.InitRobots.ForEach(r => Destroy(r.Object.transform.parent.gameObject));
            AIConfig.InitRobots = null;
        }
    }

    void OnApplicationQuit()
    {
        if (aiPerformanceWriter != null) aiPerformanceWriter.Close();
        if (aiWriter != null) aiWriter.Close();
        if(robotWriter != null) robotWriter.Close();
    }

}
