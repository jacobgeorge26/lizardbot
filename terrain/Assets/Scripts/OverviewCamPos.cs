using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OverviewCamPos : MonoBehaviour
{
    //Documentation - https://forum.unity.com/threads/click-drag-camera-movement.39513/
    public float dragSpeed = 2;
    private Vector3 dragOrigin;

    private float noTerrains, minHeight, maxHeight, minEdge, maxEdge;

    void Start()
    {
        noTerrains = Mathf.CeilToInt(AIConfig.PopulationSize / 25f);
        int grid = Math.Max(1, Mathf.CeilToInt(Mathf.Sqrt(noTerrains)));
        minHeight = 20f;
        maxHeight = noTerrains * TerrainConfig.GetTerrainWidth() / 2;
        minEdge = AIConfig.SpawnPoints[0].x - (TerrainConfig.GetTerrainWidth() / 2);
        maxEdge = minEdge + (grid * TerrainConfig.GetTerrainWidth()) + ((grid - 1) * TerrainConfig.Gap);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!(Input.GetMouseButton(0) || Input.mouseScrollDelta.y != 0)) return;
        Vector3 pos, move = Vector3.zero;
        if(Input.mouseScrollDelta.y != 0)
        {
            //scroll - zoom in or out
            pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            move = new Vector3(0, Input.mouseScrollDelta.y * -dragSpeed, 0);
            move.y = move.y > 0 && this.transform.position.y > maxHeight ? 0 : move.y;
            move.y = move.y < 0 && this.transform.position.y < minHeight ? 0 : move.y;
        }
        else
        {
            //drag - move around screen
            pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);
            if (move.x != 0f)
            {
                move.x = move.x > 0 && this.transform.position.x > maxEdge ? 0 : move.x;
                move.x = move.x < 0 && this.transform.position.x < minEdge ? 0 : move.x;
            }
            if (move.z != 0f)
            {
                move.z = move.z > 0 && this.transform.position.z > maxEdge ? 0 : move.z;
                move.z = move.z < 0 && this.transform.position.z < minEdge ? 0 : move.z;
            }
        }
        transform.Translate(move, Space.World);

    }
}
