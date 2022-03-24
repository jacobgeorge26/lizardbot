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
        while(IsEnabled && robot.IsEnabled)
        {
            yield return new WaitForSeconds(DynMovConfig.AdjustRate);
            Vector3 newPosition = robot.GetAveragePosition(); //worldspace, not local
            Vector3 spawnPoint = TerrainConfig.SpawnPoints[Mathf.FloorToInt(robot.RobotIndex / 25)];
            List<Vector3> adjustedVectors = AdjustSphere(newPosition);
            Vector3 forwardVector = newPosition - spawnPoint; //which direction is most efficiently away from the spawn point
            //filter for the adjustment sensitivity, then order by how close to this vector each sphere point is
            List<Vector3> filteredVectors = adjustedVectors.Where(p => Vector3.Angle(p, forwardVector) <= DynMovConfig.AdjustSensitivity).OrderBy(p => Vector3.Angle(p, forwardVector)).ToList();
            for (int i = filteredVectors.Count - 1; i > 0; i--)
            {
                int index = adjustedVectors.IndexOf(filteredVectors[i]);
                if (robot.Configs.Where(o => {
                    if (o.Type == BodyPart.Body && o.Body != null) return o.Body.Velocities[index].magnitude > 0;
                    else if (o.Type == BodyPart.Tail && o.Tail != null) return o.Tail.Velocities[index].magnitude > 0;
                    else return false;
                }).ToList().Count == 0)
                {
                    filteredVectors.Remove(filteredVectors[i]);
                }
            }
            foreach (Vector3 item in adjustedVectors)
            {
                GameObject turn = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
                turn.transform.localScale = new Vector3(1f, 1f, 1f);
                turn.transform.position = item;
            }
            Vector3 pointVector = DynMovConfig.SpherePoints.OrderBy(p => Vector3.Angle(p, new Vector3(0, 0, 90))).First();
            GameObject point = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
            point.transform.localScale = new Vector3(1f, 1f, 1f);
            point.transform.position = pointVector + newPosition;
            //if(filteredVectors.Count > 0)
            //{
            //    Vector3 point = filteredVectors.First();
            //    int index = adjustedVectors.IndexOf(point);
            //    ////////////////////////

            //    robot.SetDynMovVelocities(index);
            //}
        }
        IsEnabled = false;
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
            List<Vector3> adjustedVectors = AdjustSphere(newPosition);
            Vector3 movementVector = newPosition - oldPosition;
            //get sphere point closest to the movement Vector
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

    private List<Vector3> AdjustSphere(Vector3 centre)
    {
        Vector3[] adjustedVectors = new Vector3[DynMovConfig.NoSphereSamples];
        Vector3 rotation = robot.GetAverageRotation(); //worldspace, not local
        for (int i = 0; i < DynMovConfig.NoSphereSamples; i++)
        {
            adjustedVectors[i] = RotatePointAroundPivot(DynMovConfig.SpherePoints[i] + centre, centre, rotation);
        }
        return adjustedVectors.ToList();
    }

    //This method was taken from this unity forum post
    //https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

}
