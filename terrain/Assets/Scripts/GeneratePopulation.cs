using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GeneratePopulation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateRobots());
    }

    IEnumerator GenerateRobots()
    {
        CameraConfig.OverviewCamera = Camera.main.gameObject;
        CameraConfig.Hat = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Hat"));
        CameraConfig.Hat.name = "hat";
        CameraConfig.Hat.SetActive(false);
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "camera";
        cam.SetActive(false);
        CameraConfig.RobotCamera = cam;
        for (int i = 0; i < AIConfig.PopulationSize; i++)
        {
            GameObject robot = new GameObject();
            robot.name = $"Robot {i + 1}";
            GameObject version = new GameObject();
            version.transform.parent = robot.transform;
            version.AddComponent<GenerateRobot>();
            yield return new WaitForSeconds(1f);
        }

    }
}
