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
    float startTime = 0;

    private static UIDisplay ui;

    void Start()
    {
        PlayerPrefs.SetInt("Attempt", 33);
        Controller = this;
        attemptCount = AIConfig.NoAttempts;
        //setup writers with headers
        string filePath = "../terrain/Report/Data/AIPerformance.csv";
        aiPerformanceWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        aiPerformanceWriter.WriteLine($"ATTEMPT, Time, Performance");
        filePath = "../terrain/Report/Data/AIConfig.csv";
        aiWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        aiWriter.WriteLine($"ATTEMPT, {AIConfig.GetHeader()}");
        StartCoroutine(GenerateAttempt(false));
    }

    private IEnumerator GenerateAttempt(bool isRespawn)
    {
        if (AIConfig.PopulationSize > 0)
        {         
            for (int i = attemptCount; i > 0; i--)
            {
                //IMPORTANT delay needed to allow unity to stop running scripts attached to newly disabled objects
                yield return new WaitForSeconds(5f);
                attemptCount--;
                UpdateAttempt(1);
                if(!isRespawn) SetupAIParams();
                generate = gameObject.AddComponent<GeneratePopulation>();
                Debug.Log($"ATTEMPT {PlayerPrefs.GetInt("Attempt")}");
                generate.CreatePopulation();
                StartCoroutine(SaveAttemptData(isRespawn));
                yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime >= AIConfig.AttemptLength);
                AIConfig.InitRobots.ForEach(r => { if (r.Object != null) r.Object.SetActive(false); });
                AIConfig.InitRobots.Clear(); //if in the middle of a respawn, scrap that
                AIConfig.RobotConfigs.ForEach(r => {
                    r.Object.transform.parent.gameObject.SetActive(false);
                });
                if (AIConfig.LogRobotData) LogBestRobot();
                StartCoroutine(TotalDeconstruct());
            }
            Application.Quit();
        }
    }

    private bool firstRandom = false;
    private void SetupAIParams()
    {
        int noRandoms = firstRandom ? 1 : 2;
        //run twice the first time because for some reason it produces the same results the first time it's run
        for (int i = 0; i < noRandoms; i++)
        {
            firstRandom = true;
            AIConfig.MutationCycle = (int)Random.Range(0, 10);
            AIConfig.RecombinationRate = Random.value;
            AIConfig.MutationRate = Random.value;
            AIConfig.SelectionSize = Random.Range(1, 20);
        }
        int attempt = PlayerPrefs.GetInt("Attempt");
        aiWriter.WriteLine($"{attempt}, {AIConfig.GetData()}");
    }

    private IEnumerator SaveAttemptData(bool isRespawn)
    {
        int attempt = PlayerPrefs.GetInt("Attempt");
        if(!isRespawn) startTime = Time.realtimeSinceStartup;
        while (PlayerPrefs.GetInt("Attempt") == attempt)
        {
            yield return new WaitForSeconds(10f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {               
                List<RobotConfig> ordered = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).ToList();
                int take = Mathf.CeilToInt(AIConfig.PopulationSize / 4f);
                float performance = ordered.Take(take).Average(r => r.Performance);
                aiPerformanceWriter.WriteLine($"{attempt}, {Time.realtimeSinceStartup - startTime}, {performance}");
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
        AIConfig.InitRobots = AIConfig.RobotConfigs;
        StartCoroutine(TotalDeconstruct());
        //get ready to restart attempt
        attemptCount++;
        UpdateAttempt(-1);
        StartCoroutine(GenerateAttempt(true));
    }

    internal void SingleRespawn(string exception, RobotConfig robot)
    {
        //stop execution for this robot - an error has occurred
        if(robot.Object == null || AIConfig.PopulationSize == AIConfig.InitRobots.Count - 1)
        {
            //if there is an issue getting the associated robot objects, or all the robots are malfunctioning
            //do a total respawn instead
            TotalRespawn(exception);
            return;
        }       
        robot.Object.gameObject.SetActive(false);
        ui ??= UIConfig.UIContainer.GetComponent<UIDisplay>();
        if (ui != null)
        {
            //move to overview cam and then disable until respawn is complete
            ui.SelectOption(UIView.Performance);
            ui.Disable();
        }
        AIConfig.IsTotalRespawn = false;
        Debug.LogWarning($"Respawning robot {robot.RobotIndex + 1} in attempt {PlayerPrefs.GetInt("Attempt")}. \n {exception}");
        AIConfig.InitRobots.Add(robot);
        if (AIConfig.InitRobots.Count == 1) StartCoroutine(ReenableUI()); //only needed if this is the first in a cluster of single respawns
        StartCoroutine(generate.RespawnRobot(robot));
    }

    private IEnumerator ReenableUI()
    {
        ui ??= UIConfig.UIContainer.GetComponent<UIDisplay>();
        if (ui != null && UIConfig.IsUIEnabled)
        {
            yield return new WaitUntil(() => AIConfig.InitRobots.Count == 0);
            ui.Enable();
        }
    }

    private IEnumerator TotalDeconstruct()
    {
        ui ??= UIConfig.UIContainer.GetComponent<UIDisplay>();
        if(ui != null)
        {
            //move to overview cam and then disable until respawn is complete
            ui.SelectOption(UIView.Performance);
            ui.Disable();
        }
        Destroy(CameraConfig.RobotCamera);
        Destroy(CameraConfig.Hat);
        Destroy(generate);
        AIConfig.RobotConfigs.ForEach(r => {
            r.Configs.ForEach(o => {
                if(AIConfig.InitRobots.Count == 0)
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
        AIConfig.IsTotalRespawn = true;
        if (AIConfig.InitRobots.Count > 0)
        {
            yield return new WaitUntil(() => AIConfig.InitRobots.Count == AIConfig.RobotConfigs.Count);
            AIConfig.InitRobots = new List<RobotConfig>();
        }
    }

    void OnApplicationQuit()
    {
        if (aiPerformanceWriter != null) aiPerformanceWriter.Close();
        if (aiWriter != null) aiWriter.Close();
        if(robotWriter != null) robotWriter.Close();
    }

}
