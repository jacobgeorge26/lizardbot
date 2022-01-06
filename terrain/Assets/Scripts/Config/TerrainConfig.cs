using ProceduralToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public static class TerrainConfig : object
    {
        public static Surface SurfaceType { get; set; } = Surface.Uneven;

        public static float CellSize = 0.5f;

        private static int TerrainWidth = 100;

        public static Vector3 GetTerrainSize()
        {
            return new Vector3(TerrainWidth, GetTerrainHeight(), TerrainWidth);
        }

        public static float GetTerrainHeight()
        {
            return 0f;
            return (int)SurfaceType * 8f;
        }

        public static float GetNoiseFrequency()
        {
            return ((int)SurfaceType * TerrainWidth * 0.02f) + 0.04f;
        }

        public static Gradient GetGradient()
        {
            Color darkGreen = new Color(0.027f, 0.368f, 0.076f, 1f);
            Color darkBrown = new Color(0.3647f, 0.2275f, 0.102f, 1f);
            Color lightGrey = new Color(0.690f, 0.714f, 0.745f, 1f);
            Color darkYellow = new Color(0.952f, 0.69f, 0.349f, 1f);
            Color lightYellow = new Color(0.283f, 0.069f, 0.036f, 1f);
            switch (SurfaceType)
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
    }
}

