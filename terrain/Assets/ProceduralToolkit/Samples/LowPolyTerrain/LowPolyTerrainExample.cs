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
        public LowPolyTerrainGenerator.Config config = new LowPolyTerrainGenerator.Config();

        private Mesh terrainMesh;
        private int count = 0;
        private bool uneven = false, smooth = false;
        private void Awake()
        {
            LowPolyTerrainGenerator.SetupParams(config);
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

                config.gradient = ColorE.Gradient(from: GetMainColorHSV(), to: GetSecondaryColorHSV());
            }

            var draft = LowPolyTerrainGenerator.TerrainDraft(config);
            draft.Move(Vector3.left*config.terrainSize.x/2 + Vector3.back*config.terrainSize.z/2);
            AssignDraftToMeshFilter(draft, terrainMeshFilter, ref terrainMesh);
            terrainMeshCollider.sharedMesh = terrainMesh;
        }
    }
}
