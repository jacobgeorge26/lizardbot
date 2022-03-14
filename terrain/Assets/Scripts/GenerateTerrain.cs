using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    private int grid, noTerrains = Mathf.CeilToInt(AIConfig.PopulationSize / 25f);
    void Start()
    {
        grid = Math.Max(1, Mathf.CeilToInt(Mathf.Sqrt(noTerrains)));
        CameraConfig.OverviewCamera = Camera.main.gameObject;
        CameraConfig.OverviewCamera.name = "Overview Camera";
        for (int i = 0; i < Math.Max(1, noTerrains); i++)
        {
            GameObject terrain = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Terrain"));
            terrain.name = $"Terrain {i + 1}";
            terrain.transform.parent = gameObject.transform;
            terrain.transform.position = GetPosition(i, terrain);
        }
        float min = AIConfig.SpawnPoints[0].x - (TerrainConfig.GetTerrainWidth() / 2);
        float max = min + (grid * TerrainConfig.GetTerrainWidth()) + ((grid - 1) * TerrainConfig.Gap);
        float centre = (min + max) / 2;
        CameraConfig.OverviewCamera.transform.position = new Vector3(centre, noTerrains * TerrainConfig.GetTerrainWidth() / 4, centre);
        CameraConfig.OverviewCamera.GetComponent<Camera>().farClipPlane = (noTerrains * TerrainConfig.GetTerrainWidth() / 2) + 30;
        if (AIConfig.PopulationSize > 0)
        {
            GameObject robotController = new GameObject(name = "Robot Controller");
            GeneratePopulation generate = robotController.AddComponent<GeneratePopulation>();
            generate.CreatePopulation();
        }
        else
        {
            UIDisplay ui = FindObjectOfType<UIDisplay>();
            if (ui != null) ui.gameObject.SetActive(false);
        }
    }

    public Vector3 GetPosition(int index, GameObject terrain)
    {
        Vector3 current = terrain.transform.position;
        int row = Mathf.FloorToInt(index / grid);
        int col = index % grid;
        Vector3 newLoc = new Vector3(current.x + (col * (TerrainConfig.GetTerrainWidth() + TerrainConfig.Gap)), 0, current.z + (row * (TerrainConfig.GetTerrainWidth() + TerrainConfig.Gap)));
        AIConfig.SpawnPoints[index] = newLoc;
        return newLoc;
    }

}
