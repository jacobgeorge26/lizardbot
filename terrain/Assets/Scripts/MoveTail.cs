using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveTail : MonoBehaviour
{
    private Rigidbody tail;
    private Vector3 initCOG;
    // Start is called before the first frame update
    void Awake()
    {
        tail = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //get the current centre of gravity relative to the initial centre of gravity
        //swing the tail to counterbalance it
        Vector3 angleVelocity = GetTailVector();
        for (int i = 0; i < 2; i++)
        {
            //adjust for rotation multiplier
            angleVelocity[i] *= TailConfig.JointConfig.RotationMultiplier.Value[i];
        }
        angleVelocity.x *= -1;
        angleVelocity.y *= -1;
        tail.AddRelativeForce(angleVelocity * 0.1f, ForceMode.Force);
    }

    private Vector3 GetTailVector()
    {
        //get the average vector between each section
        Vector3 sum = new Vector3();
        for (int i = 1; i < BaseConfig.NoSections.Value; i++)
        {
            GameObject prevSection = BaseConfig.Sections[i - 1];
            GameObject section = BaseConfig.Sections[i];
            sum += GetRelativeAngle(prevSection) - GetRelativeAngle(section);
        }
        Vector3 bodyRotation = sum / BaseConfig.NoSections.Value;
        //reverse the x & y axis
        //TODO: when implementing jumping - have it so that the tail flicks up/down sharply
        //bodyRotation.x *= -1;
        bodyRotation.y *= -1;
        //current tail rotation doesn't need to be considered as it the force is applied to the tail's space rather than directly set
        return bodyRotation;
    }

    public void SetInitCOG(Vector3 _initCOG)
    {
        initCOG = _initCOG;
    }

    //return angle relative to body within range -180 -> 180. 
    //rounds to int by default as common use of this method is validation about whether to continue turning. 
    public Vector3 GetRelativeAngle(GameObject bodyObject, bool round = true)
    {
        Vector3 angle = bodyObject.transform.localRotation.eulerAngles;

        //update for range -180 - 180
        angle.x -= Math.Round(angle.x, 0) > 180 ? 360 : 0;
        angle.y -= Math.Round(angle.y, 0) > 180 ? 360 : 0;
        angle.z -= Math.Round(angle.z, 0) > 180 ? 360 : 0;

        angle.x += Math.Round(angle.x, 0) < -180 ? 360 : 0;
        angle.y += Math.Round(angle.y, 0) < -180 ? 360 : 0;
        angle.z += Math.Round(angle.z, 0) < -180 ? 360 : 0;

        //if opted to then round the angles
        angle = round ? new Vector3((float)Math.Round(angle.x, 0), (float)Math.Round(angle.y, 0), (float)Math.Round(angle.z, 0)) : angle;

        return angle;
    }

    private Vector3 GetCOG()
    {
        Vector3 sumMassByPos = new Vector3();
        float sumMass = 0;
        //iterate objects, get sum of (mass * axis coordinate), divide by sum of all masses
        foreach(GameObject section in BaseConfig.Sections)
        {
            Vector3 position = section.transform.position;
            float mass = section.GetComponent<Rigidbody>().mass;
            sumMass += mass;
            for (int i = 0; i < 2; i++)
            {
                sumMassByPos[i] += mass * position[i];
            }
        }
        return sumMassByPos / sumMass;
    }
}
