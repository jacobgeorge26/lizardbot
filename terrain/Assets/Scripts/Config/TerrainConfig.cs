using ProceduralToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Config
{
    public static class TerrainConfig : object
    {
        private static Queue<Surface> SurfaceType { get; set; } = new Queue<Surface> ( new Surface[] { Surface.Uneven} );

        private static int[] Surfaces;

        public static float CellSize = 0.5f;

        public static int Gap = 10;

        private static int TerrainWidth = 100;

        public static int NoTerrains = Mathf.CeilToInt(AIConfig.PopulationSize / 25f);

        private static Vector3[] SpawnPoints = new Vector3[Mathf.CeilToInt(AIConfig.PopulationSize / 25f)];

        public static MeshCollider[] Meshes = new MeshCollider[Mathf.CeilToInt(AIConfig.PopulationSize / 25f)];

        public static int GetTerrainWidth()
        {
            return TerrainWidth;
        }

        public static Vector3 GetTerrainSize(int index)
        {
            return new Vector3(TerrainWidth, GetTerrainHeight(index), TerrainWidth);
        }

        public static void SetupSurfaces()
        {
            Surfaces = new int[NoTerrains];
        }

        public static void SetTerrainType(int index)
        {
            Surface surface = SurfaceType.Dequeue();
            SurfaceType.Enqueue(surface);
            Surfaces[index] = (int)surface;
        }

        public static Surface GetTerrainType(int robotIndex)
        {
            int index = Mathf.FloorToInt(robotIndex / 25);
            return (Surface)Surfaces[index];
        }
        public static void SetSpawnPoint(int index, Vector3 newLoc)
        {
            SpawnPoints[index] = newLoc;
        }

        public static Vector3 GetSpawnPoint(int robotIndex)
        {
            return SpawnPoints[Mathf.FloorToInt(robotIndex / 25)];
        }

        public static void SetTerrainMesh(int index, MeshCollider mesh)
        {
            Meshes[index] = mesh;
        }

        public static MeshCollider GetTerrainMesh(int robotIndex)
        {
            return Meshes[Mathf.FloorToInt(robotIndex / 25)];
        }

        public static float GetTerrainHeight(int index)
        {
            return Surfaces[index] * 8f;
        }

        public static float GetNoiseFrequency(int index)
        {
            return (Surfaces[index] * TerrainWidth * 0.02f) + 0.04f;
        }

        public static Gradient GetGradient(int index)
        {
            Color darkGreen = new Color(0.027f, 0.368f, 0.076f, 1f);
            Color darkBrown = new Color(0.3647f, 0.2275f, 0.102f, 1f);
            Color lightGrey = new Color(0.690f, 0.714f, 0.745f, 1f);
            Color darkYellow = new Color(0.952f, 0.69f, 0.349f, 1f);
            Color sandyRed = new Color(0.867f, 0.389f, 0.314f, 1f);
            Color lightRed = new Color(1f, 0.58f, 0.46f, 1f);
            Color lightYellow = new Color(0.283f, 0.069f, 0.036f, 1f);
            switch ((Surface)Surfaces[index])
            {
                case Surface.Smooth:
                    return ColorE.Gradient(lightYellow, darkYellow);
                case Surface.Uneven:
                    return ColorE.Gradient(darkGreen, darkBrown);
                case Surface.Rough:
                    return ColorE.Gradient(lightGrey, Color.black);
                default:
                    return ColorE.Gradient(Color.black, Color.white);
            }
        }

        public static float minX() {
            return SpawnPoints.Min(p => p.x) - (TerrainConfig.GetTerrainWidth() / 2);
        }
        public static float maxX() {
            return SpawnPoints.Max(p => p.x) + (TerrainConfig.GetTerrainWidth() / 2);
        }
        public static float minY() {
            return 20f;
        }
        public static float maxY() {
            return Math.Max(100f, NoTerrains * TerrainConfig.GetTerrainWidth() / 2);
        }
        public static float minZ() {
            return SpawnPoints.Min(p => p.z) - (TerrainConfig.GetTerrainWidth() / 2);
        }
        public static float maxZ() {
            return SpawnPoints.Max(p => p.z) + (TerrainConfig.GetTerrainWidth() / 2);
        }
    }
}

