using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePopulation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateRobots());
    }

    IEnumerator GenerateRobots()
    {
        for (int i = 0; i < AIConfig.PopulationSize; i++)
        {
            GameObject robot = new GameObject();
            robot.AddComponent<GenerateRobot>();
            yield return new WaitForSeconds(1f);
        }

    }
}
