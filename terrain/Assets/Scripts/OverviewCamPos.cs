using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OverviewCamPos : MonoBehaviour
{
    //Documentation - https://docs.unity3d.com/ScriptReference/Vector3.Lerp.html
    void Update()
    {
        Transform oldPos = CameraConfig.OverviewCamera.transform;
        int minHeight = 30;
        Vector3 newPos = new Vector3(0, minHeight, 0);
        Vector3 origin = new Vector3(0, TerrainConfig.GetTerrainHeight() / 2, 0);
        Collider[] allColliders = Physics.OverlapBox(origin, TerrainConfig.GetTerrainSize() / 2);
        for (int i = TerrainConfig.GetTerrainWidth() - 10; i > 0; i -= 10)
        {
            Collider[] innerColliders = Physics.OverlapBox(origin, new Vector3(i / 2, TerrainConfig.GetTerrainHeight() / 2, i / 2));
            var outsideRadius = allColliders.Except(innerColliders);
            if(outsideRadius.Count() > 0)
            {
                newPos = new Vector3(0, Math.Max(minHeight, i + 20), 0);
                break;
            }
        }
        float length = Vector3.Distance(oldPos.position, newPos);
        if(length > 1)
        {
            CameraConfig.OverviewCamera.transform.position = Vector3.Lerp(oldPos.position, newPos, Time.deltaTime);
        }
    }
}
