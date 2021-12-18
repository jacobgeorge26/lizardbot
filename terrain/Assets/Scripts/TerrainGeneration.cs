using Config;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    /*
     uses https://www.youtube.com/watch?v=vFvwyu_ZKfU
     */

    private int width = 129;
    private int length = 129;
    private int depth;

    private float scale; //> -> more ridges more frequently
    //keeping static for now - can be made dynamic later if desired


    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        TerrainConfig terrainConfig = FindObjectOfType<TerrainConfig>();
        if(terrainConfig == null) Debug.LogError("The terrain materials haven't been set up");
        
        switch (BaseConfig.SurfaceType)
        {
            case Surface.Smooth:
                depth = 2;
                scale = 25;
                if(terrainConfig != null && terrainConfig.SmoothTexture != null)
                {
                    terrain.materialTemplate = terrainConfig.SmoothTexture;
                }
                else
                {
                    Debug.LogError("The smooth terrain material is null");
                }
                break;
            case Surface.Uneven:
                depth = 4;
                scale = 50;
                if (terrainConfig != null && terrainConfig.UnevenTexture != null)
                {
                    terrain.materialTemplate = terrainConfig.UnevenTexture;
                }
                else
                {
                    Debug.LogError("The uneven terrain material is null");
                }
                break;
            case Surface.Rough:
                depth = 6;
                scale = 75;
                if (terrainConfig != null && terrainConfig.RoughTexture != null)
                {
                    terrain.materialTemplate = terrainConfig.RoughTexture;
                }
                else
                {
                    Debug.LogError("The rough terrain material is null");
                }
                break;
        }
        //TODO: make terrain neighbours dynamically
    }

    private TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, length);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[width, length];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < length; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    private float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCord = (float)y / length * scale;

        return Mathf.PerlinNoise(xCoord, yCord);
    }
}
