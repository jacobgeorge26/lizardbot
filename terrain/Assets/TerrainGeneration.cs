using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    /*
     uses https://www.youtube.com/watch?v=vFvwyu_ZKfU
     */
    public int width;
    public int length;
    public int depth;

    public float scale = 20;

    public float offsetX = 100f;
    public float offsetY = 100f;
    void Update()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
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
        float xCoord = (float)x / width * scale + offsetX;
        float yCord = (float)y / length * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCord);
    }
}
