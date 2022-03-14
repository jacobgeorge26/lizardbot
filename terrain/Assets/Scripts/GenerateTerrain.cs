using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    private int grid;
    void Start()
    {
        grid = Math.Max(1, Mathf.CeilToInt(Mathf.Sqrt(TerrainConfig.NoTerrains)));
        CameraConfig.OverviewCamera = Camera.main.gameObject;
        CameraConfig.OverviewCamera.name = "Overview Camera";
        TerrainConfig.SetupSurfaces();
        for (int i = 0; i < Math.Max(1, TerrainConfig.NoTerrains); i++)
        {
            GameObject terrain = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Terrain"));
            terrain.SetActive(false);
            terrain.name = $"Terrain {i + 1}";
            terrain.transform.parent = gameObject.transform;
            terrain.transform.position = GetPosition(i, terrain);
            terrain.SetActive(true);
        }
        CameraConfig.OverviewCamera.transform.position = new Vector3((TerrainConfig.minX() + TerrainConfig.maxX()) / 2, Math.Max(30f, TerrainConfig.NoTerrains * TerrainConfig.GetTerrainWidth() / 4), (TerrainConfig.minZ() + TerrainConfig.maxZ()) / 2);
        CameraConfig.OverviewCamera.GetComponent<Camera>().farClipPlane = (TerrainConfig.NoTerrains * TerrainConfig.GetTerrainWidth() / 2) + 30;
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
