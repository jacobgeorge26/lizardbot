using Config;
using ProceduralToolkit.Samples.UI;
using UnityEngine;

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

        private Mesh terrainMesh;
        private void Awake()
        {
            Generate();
            SetupSkyboxAndPalette();
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

            var draft = LowPolyTerrainGenerator.TerrainDraft();
            draft.Move(Vector3.left*TerrainConfig.GetTerrainSize().x/2 + Vector3.back*TerrainConfig.GetTerrainSize().z/2);
            AssignDraftToMeshFilter(draft, terrainMeshFilter, ref terrainMesh);
            terrainMeshCollider.sharedMesh = terrainMesh;
        }
    }
}
