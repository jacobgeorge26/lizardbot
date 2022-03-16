using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{

    StreamWriter performanceWriter, aiWriter;
    int attemptTime = 600;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateAttempt());
    }

    private IEnumerator GenerateAttempt()
    {
        if (AIConfig.PopulationSize > 0)
        {
            string filePath = "../terrain/Report/Data/AIPerformance.csv";
            performanceWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
            performanceWriter.WriteLine($"ATTEMPT, Time, Performance");
            filePath = "../terrain/Report/Data/AIConfig.csv";
            aiWriter = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
            aiWriter.WriteLine($"ATTEMPT, {AIConfig.GetHeader()}");
            for (int i = 0; i < AIConfig.NoAttempts; i++)
            {
                SetupAIParams();
                GeneratePopulation generate = gameObject.AddComponent<GeneratePopulation>();
                generate.UpdateAttempt();
                Debug.Log($"ATTEMPT {PlayerPrefs.GetInt("Attempt")}");
                generate.CreatePopulation();
                StartCoroutine(SaveAttemptData());
                yield return new WaitForSeconds(attemptTime);
                Destroy(generate);
                Destroy(CameraConfig.RobotCamera);
                Destroy(CameraConfig.Hat);
                AIConfig.RobotConfigs.ForEach(r => {
                    Destroy(r.Object.transform.parent.gameObject);
                    r.Configs.ForEach(o => {
                        o.Body = null;
                        o.Tail = null;
                    });
                });
                AIConfig.RobotConfigs = new List<RobotConfig>();
                AIConfig.BestRobot = null;
                AIConfig.LastRobots = new RobotConfig[AIConfig.PopulationSize];
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
        float startTime = Time.realtimeSinceStartup;
        while (PlayerPrefs.GetInt("Attempt") == attempt)
        {
            yield return new WaitForSeconds(10f);
            if (AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {               
                List<RobotConfig> ordered = AIConfig.RobotConfigs.OrderByDescending(r => r.Performance).ToList();
                float performance = ordered.Take(AIConfig.PopulationSize / 4).Average(r => r.Performance);
                performanceWriter.WriteLine($"{attempt}, {Time.realtimeSinceStartup - startTime}, {performance}");
            }
        }

    }

    void OnApplicationQuit()
    {
        if (performanceWriter != null) performanceWriter.Close();
        if (aiWriter != null) aiWriter.Close();
    }

}
