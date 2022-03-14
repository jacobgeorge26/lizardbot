using Config;
using ProceduralToolkit.Samples.UI;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
namespace ProceduralToolkit.Samples
{
    /// <summary>
    /// Configurator for LowPolyTerrainGenerator with UI and editor controls
    /// </summary>
    public class LowPolyTerrainExample : ConfiguratorBase
    {
        public MeshFilter terrainMeshFilter;
        public MeshCollider terrainMeshCollider;
        private bool constantSeed = true;
        private int index;

        private Mesh terrainMesh;
        private void Awake()
        {
            //really bad way of doing this - improve on it later
            index = Int32.Parse(Regex.Match(gameObject.name, @"\d+").Value) - 1;
            TerrainConfig.SetTerrainType(index);
            Generate();
            SetupSkyboxAndPalette();
            Destroy(this);
        }

        public void Generate(bool randomizeConfig = false)
        {
            if (constantSeed)
            {
                Random.InitState(0);
            }

            if (randomizeConfig)
            {
                GeneratePalette();
            }

            var draft = LowPolyTerrainGenerator.TerrainDraft(index);
            draft.Move(Vector3.left*TerrainConfig.GetTerrainSize(index).x/2 + Vector3.back*TerrainConfig.GetTerrainSize(index).z/2);
            AssignDraftToMeshFilter(draft, terrainMeshFilter, ref terrainMesh);
            terrainMeshCollider.sharedMesh = terrainMesh;
        }


    }
}
