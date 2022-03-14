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
        Vector3 newLoc = new Vector3(current.x + (col * TerrainConfig.GetTerrainWidth()) + 20, 0, current.z + (row * TerrainConfig.GetTerrainWidth()) + 20);
        if (Mathf.CeilToInt(noTerrains / 2) == index - 1){
            CameraConfig.OverviewCamera.transform.position = new Vector3(newLoc.x, TerrainConfig.GetTerrainHeight() + 50, newLoc.z);
        }
        AIConfig.SpawnPoints[index] = newLoc;
        return newLoc;
    }

}
