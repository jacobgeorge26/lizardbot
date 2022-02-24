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
        //setup camera
        CameraConfig.OverviewCamera = Camera.main.gameObject;
        CameraConfig.OverviewCamera.name = "Overview Camera";
        CameraConfig.Hat = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Hat"));
        CameraConfig.Hat.name = "hat";
        CameraConfig.Hat.SetActive(false);
        GameObject cam = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Robot Camera"));
        cam.name = "Robot Camera";
        cam.SetActive(false);
        CameraConfig.RobotCamera = cam;
        //generate population, leaving a gap between them
        for (int i = 0; i < AIConfig.PopulationSize; i++)
        {
            //setup UI after first robot has been generated
            if(i == 1)
            {
                //setup UI
                UI ui = FindObjectOfType<UI>();
                if (ui != null && UIConfig.IsUIEnabled) ui.enabled = true;
                else UIConfig.UIContainer.SetActive(false);
            }
            GameObject robot = new GameObject();
            robot.name = $"Robot {i + 1}";
            GameObject version = new GameObject();
            version.transform.parent = robot.transform;
            version.AddComponent<GenerateRobot>();
            yield return new WaitForSeconds(1f);
        }
        //destroy this script now that it's finished
        Destroy(this);
    }
}
