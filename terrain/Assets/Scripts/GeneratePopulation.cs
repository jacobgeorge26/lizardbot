using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePopulation : MonoBehaviour
{
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
        //destroy this script now that it's finished
        Destroy(this.gameObject);
    }
}
