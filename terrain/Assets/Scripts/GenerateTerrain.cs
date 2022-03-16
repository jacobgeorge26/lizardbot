using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    private int grid;
    void Awake()
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
        CameraConfig.OverviewCamera.transform.position = new Vector3((TerrainConfig.minX() + TerrainConfig.maxX()) / 2, (TerrainConfig.minY() + TerrainConfig.maxY()) / 2, (TerrainConfig.minZ() + TerrainConfig.maxZ()) / 2);
        CameraConfig.OverviewCamera.GetComponent<Camera>().farClipPlane = TerrainConfig.maxY() + 20;
        UIDisplay ui = FindObjectOfType<UIDisplay>();
        if (ui != null) ui.gameObject.SetActive(false);
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
