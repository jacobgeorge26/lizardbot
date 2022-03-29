using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicMovement : MonoBehaviour
{
    RobotConfig robot;
    private bool IsEnabled = false;

    void Start()
    {
        IsEnabled = false;
    }

    void Update()
    {
        if(robot == null)
        {
            //get robot config
            foreach (Transform child in this.gameObject.transform)
            {
                ObjectConfig objConfig = child.gameObject.GetComponent<ObjectConfig>();
                if(objConfig != null)
                {
                    try { robot = AIConfig.RobotConfigs.First(r => r.RobotIndex == objConfig.RobotIndex); }
                    catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }
                    break;
                }
            }
        }
        if (!IsEnabled && robot != null && robot.IsEnabled)
        {
            StartCoroutine(SampleVelocity());
            StartCoroutine(AdjustRobot());
        }
    }

    private IEnumerator AdjustRobot()
    {
        IsEnabled = true;
        //wait at first to allow some data collection
        yield return new WaitForSeconds(DynMovConfig.InitialDelay);
        while(IsEnabled && robot.IsEnabled)
        {
            yield return new WaitForSeconds(DynMovConfig.AdjustRate);
            MakeAdjustment(false);
        }
        IsEnabled = false;
    }

    internal void MakeAdjustment(bool forceAdjustment)
    {
        Vector3 newPosition = robot.GetAveragePosition(); //worldspace, not local
        Vector3 spawnPoint = TerrainConfig.GetSpawnPoint(robot.RobotIndex);
        Vector3 forwardVector = newPosition - spawnPoint; //which direction is most efficiently away from the spawn point

        //if the robot is already moving within 2x the accepted angle then leave it as is
        Vector3 trajectory = robot.Configs.First().gameObject.GetComponent<Rigidbody>().velocity; //TODO: improve
        trajectory.y = forwardVector.y; //ignore vertical angle difference
        if (!forceAdjustment && Vector3.Angle(forwardVector, trajectory) < DynMovConfig.AdjustSensitivity * 2) return;

        //filter for the adjustment sensitivity, then order by how close to this vector each sphere point is
        List<Vector3> adjustedVectors = AdjustVectors(spawnPoint, newPosition);
        List<Vector3> filteredVectors = adjustedVectors.Where(p => Vector3.Angle(new Vector3(p.x, 0, p.z), new Vector3(forwardVector.x, 0, forwardVector.z)) <= DynMovConfig.AdjustSensitivity).OrderBy(p => Vector3.Angle(p, forwardVector)).ToList();

        //if there aren't any velocities stored for this vector then there is no point looking there
        RemoveEmptyVelocities(filteredVectors, adjustedVectors);

        //if the vector would move in a direction below the height of the terrain then ignore it
        RemoveTooLowVectors(filteredVectors, newPosition);

        if (filteredVectors.Count > 0)
        {
            Vector3 point = filteredVectors.First();
            int index = adjustedVectors.IndexOf(point);
            robot.SetDynMovVelocities(index, forceAdjustment);
        }
    }

    private List<Vector3> AdjustVectors(Vector3 spawnPoint, Vector3 centre)
    {
        Vector3 rotation = robot.GetAverageRotation();
        Vector3[] adjusted = new Vector3[DynMovConfig.NoSphereSamples];
        for (int i = 0; i < DynMovConfig.NoSphereSamples; i++)
        {
            adjusted[i] = DynMovConfig.SpherePoints[i] + spawnPoint;
            adjusted[i] = RotatePointAroundPivot(adjusted[i], spawnPoint, rotation);
            adjusted[i] += centre;
        }
        return adjusted.ToList();
    }

    //This method was taken from this unity forum post
    //https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html
    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    private void RemoveTooLowVectors(List<Vector3> filteredVectors, Vector3 position)
    {
        RaycastHit hit;
        MeshCollider terrain = FindObjectOfType<MeshCollider>(); //TODO: improve this
        for (int index = filteredVectors.Count - 1; index > 0; index--)
        {
            Vector3 point = filteredVectors[index];
            float maxHeight = 0, multiplier = 0;
            for (int i = 1; i < 4; i++)
            {
                float distance = i * 5f;
                multiplier = Mathf.Sqrt((distance * distance) / ((point.x * point.x) + (point.z * point.z)));
                Ray ray = new Ray(new Vector3(point.x * multiplier, 20, point.z * multiplier), Vector3.down);
                if (terrain.Raycast(ray, out hit, 2.0f * 20))
                {
                    maxHeight = hit.point.y > maxHeight ? hit.point.y : maxHeight;
                }
            }
            if (maxHeight > (point.y * multiplier)) //this vector won't get over the terrain
            {
                filteredVectors.Remove(point);
            }
        }
    }

    private void RemoveEmptyVelocities( List<Vector3> filteredVectors, List<Vector3> adjustedVectors)
    {
        for (int i = filteredVectors.Count - 1; i > 0; i--)
        {
            int index = adjustedVectors.IndexOf(filteredVectors[i]);
            if (robot.Configs.Where(o =>
            {
                if (o.Type == BodyPart.Body && o.Body != null) return o.Body.Velocities[index].magnitude > 0;
                else if (o.Type == BodyPart.Tail && o.Tail != null) return o.Tail.Velocities[index].magnitude > 0;
                else return false;
            }).ToList().Count == 0)
            {
                filteredVectors.Remove(filteredVectors[i]);
            }
        }
    }

    private IEnumerator SampleVelocity()
    {
        IsEnabled = true;
        while (IsEnabled && robot.IsEnabled)
        {
            IsEnabled = true;
            //get current position of the robot, and set the current velocity for each body component
            Vector3 oldPosition = robot.GetAveragePosition(); //worldspace, not local
            robot.GetDynMovVelocities();

            yield return new WaitForSeconds(DynMovConfig.SampleRate);

            //adjust the vector points for the current rotation
            Vector3 newPosition = robot.GetAveragePosition(); //worldspace, not local
            Vector3 spawnPoint = TerrainConfig.GetSpawnPoint(robot.RobotIndex);
            Vector3 movementVector = newPosition - oldPosition;
            //get sphere point closest to the movement Vector
            List<Vector3> adjustedVectors = AdjustVectors(spawnPoint, oldPosition);
            Vector3 point = adjustedVectors.OrderBy(p => Vector3.Angle(p, movementVector)).First();

            int index = adjustedVectors.IndexOf(point);
            //if this movement covered more distance in this direction then update velocities for this index
            float distance = Vector3.Distance(oldPosition, newPosition);
            if (robot.Distances[index] < distance)
            {
                ReplaceVelocities(index, distance);
            }       
        }
        IsEnabled = false;
    }

    private void ReplaceVelocities(int index, float distance)
    {
        robot.Distances[index] = distance;
        foreach (ObjectConfig objConfig in robot.Configs)
        {
            if (objConfig.Type == BodyPart.Body && objConfig.Body != null)
            {
                objConfig.Body.Velocities[index] = objConfig.Body.CurrentVelocity;
            }
            else if (objConfig.Type == BodyPart.Tail && objConfig.Tail != null)
            {
                objConfig.Tail.Velocities[index] = objConfig.Tail.CurrentVelocity;
            }
        }
    }


}
