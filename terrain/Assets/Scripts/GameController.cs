using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
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
        /////////////////////////////////////
        PlayerPrefs.SetInt("Attempt", 0);
        Controller = this;
        attemptCount = AIConfig.NoAttempts;
        //setup writers with headers
        string filePath = "../terrain/Report/Data/AIPerformance.csv";
        aiPerformanceWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        if(DebugConfig.LogAIData) aiPerformanceWriter.WriteLine($"ATTEMPT, Time, Performance");
        filePath = "../terrain/Report/Data/AIConfig.csv";
        aiWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        if (DebugConfig.LogAIData) aiWriter.WriteLine($"ATTEMPT, {DebugConfig.GetHeader()}");
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
                if (!isRespawn && DebugConfig.LogAIData) SetupAIParams();
                generate = gameObject.AddComponent<GeneratePopulation>();
                Debug.Log($"ATTEMPT {PlayerPrefs.GetInt("Attempt")}");
                generate.CreatePopulation();
                if(DebugConfig.LogAIData) StartCoroutine(SaveAttemptData(isRespawn));
                if (!isRespawn) startTime = Time.realtimeSinceStartup;
                yield return new WaitUntil(() => Time.realtimeSinceStartup - startTime >= AIConfig.AttemptLength);
                DebugConfig.InitRobots.ForEach(r => { if (r.Object != null) r.Object.SetActive(false); });
                DebugConfig.InitRobots.Clear(); //if in the middle of a respawn, scrap that
                AIConfig.RobotConfigs.ForEach(r => {
                    r.Object.transform.parent.gameObject.SetActive(false);
                });
                StopAllCoroutines();
                if (DebugConfig.LogRobotData) LogBestRobot();
                StartCoroutine(TotalDeconstruct());
                //as all coroutines have been stopped - dodgy recursion but avoids issue if in middle of respawn
                StartCoroutine(GenerateAttempt(false)); 
            }
        }
    }

    private void SetupAIParams()
    {
        int noRandoms = (int)Random.Range(1, 5);
        //run twice the first time because for some reason it produces the same results the first time it's run
        for (int i = 0; i < noRandoms; i++)
        {
            AIConfig.MutationCycle = (int)Random.Range(0, 10);
            AIConfig.RecombinationRate = Random.Range(0.75f, 0.85f);
            AIConfig.MutationRate = Random.Range(0.15f, 0.35f);
            AIConfig.SelectionSize = Random.Range(4, 10);
        }
        int attempt = PlayerPrefs.GetInt("Attempt");
        aiWriter.WriteLine($"{attempt}, {DebugConfig.GetData()}");
    }

    private IEnumerator SaveAttemptData(bool isRespawn)
    {
        int attempt = PlayerPrefs.GetInt("Attempt");
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
        if (DebugConfig.BestRobot == null && AIConfig.RobotConfigs.Count > 0) DebugConfig.BestRobot = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).First();
        if (DebugConfig.BestRobot != null)
        {
            int attempt = PlayerPrefs.GetInt("Attempt");
            if (robotWriter == null) SetupRobotWriter();
            string data = $"{ attempt}, ";
            data += DebugConfig.GetData();
            data += DebugConfig.BestRobot.GetData();
            robotWriter.WriteLine(data);


            //save prefab
            try
            {
                GameObject robot = DebugConfig.BestRobot.Object;
                string localPath = $"Assets/Resources/Robots/{attempt}-{robot.name}.prefab";
                //save colour
                if (DebugConfig.BestRobot.NoSections.Value > 1)
                {
                    //save material too
                    try
                    {
                        List<ObjectConfig> bodies = DebugConfig.BestRobot.Configs.Where(o => o.Type == BodyPart.Body && o.Index > 0).ToList();
                        Material material = bodies.First().gameObject.GetComponent<MeshRenderer>().material;
                        foreach (var body in bodies)
                        {
                            body.gameObject.GetComponent<MeshRenderer>().material = material;
                        }
                        string materialPath = $"Assets/Resources/Robots/Materials/{attempt}-{robot.name}.mat";
                        AssetDatabase.CreateAsset(material, materialPath);
                        AssetDatabase.SaveAssets();
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"It was not possible to save the material for the prefab of the best robot for attempt {attempt}");
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(robot, localPath);
            }
            catch (Exception)
            {
                Debug.LogWarning($"It was not possible to save a prefab of the best robot for attempt {attempt}");
            }

        }
    }

    private StreamWriter SetupRobotWriter()
    {
        string filePath = "../terrain/Report/Data/BestRobots.csv";
        robotWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
        robotWriter.WriteLine();
        string header = "ATTEMPT, ";
        header += DebugConfig.GetHeader();
        header += DebugConfig.BestRobot.GetHeader();
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
        DebugConfig.InitRobots = AIConfig.RobotConfigs;
        StartCoroutine(TotalDeconstruct());
        //get ready to restart attempt
        attemptCount++;
        UpdateAttempt(-1);
        StartCoroutine(GenerateAttempt(true));
    }

    //isSuccessfulRobot is used when the robot has reached the edge of the map and is being respawned but needs to maintain its performance
    internal void SingleRespawn(string exception, RobotConfig robot)
    {
        //stop execution for this robot - an error has occurred
        if(robot.Object == null || AIConfig.PopulationSize == DebugConfig.InitRobots.Count - 1)
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
        DebugConfig.IsTotalRespawn = false;
        Debug.LogWarning($"Respawning robot {robot.RobotIndex + 1} in attempt {PlayerPrefs.GetInt("Attempt")}. \n {exception}");
        DebugConfig.InitRobots.Add(robot);
        if (DebugConfig.InitRobots.Count == 1) StartCoroutine(ReenableUI()); //only needed if this is the first in a cluster of single respawns
        StartCoroutine(generate.RespawnRobot(robot));
    }

    private IEnumerator ReenableUI()
    {
        ui ??= UIConfig.UIContainer.GetComponent<UIDisplay>();
        if (ui != null && UIConfig.IsUIEnabled)
        {
            yield return new WaitUntil(() => DebugConfig.InitRobots.Count == 0);
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
                if(DebugConfig.InitRobots.Count == 0)
                {
                    Destroy(r.Object.transform.parent.gameObject);
                    o.Body = null;
                    o.Tail = null;
                }
            });
        });
        AIConfig.RobotConfigs = new List<RobotConfig>();
        DebugConfig.BestRobot = null;
        AIConfig.LastRobots = new RobotConfig[AIConfig.PopulationSize];
        DebugConfig.IsTotalRespawn = true;
        if (DebugConfig.InitRobots.Count > 0)
        {
            yield return new WaitUntil(() => DebugConfig.InitRobots.Count == AIConfig.RobotConfigs.Count);
            DebugConfig.InitRobots = new List<RobotConfig>();
        }
    }

    void OnApplicationQuit()
    {
        if (aiPerformanceWriter != null) aiPerformanceWriter.Close();
        if (aiWriter != null) aiWriter.Close();
        if(robotWriter != null) robotWriter.Close();
    }

}
