using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveTail : MonoBehaviour
{
    private Rigidbody tail;
    private TailConfig config;
    private Vector3 initCOG;
    // Start is called before the first frame update
    void Awake()
    {
        tail = GetComponent<Rigidbody>();
        config = GetComponent<TailConfig>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //get the current centre of gravity relative to the initial centre of gravity
        //swing the tail to counterbalance it
        Vector3 relativeAngle = GetCOG() - initCOG;
        Vector3 angleVelocity = relativeAngle * -1;
        for (int i = 0; i < 2; i++)
        {
            //adjust for rotation multiplier
            angleVelocity[i] *= config.RotationMultiplier[i];
        }
        //convert vector to a quaternion
        Quaternion deltaRotation = Quaternion.Euler(angleVelocity * Time.fixedDeltaTime);
        //apply the vector to the body's space and rotate it
        tail.MoveRotation(tail.rotation * deltaRotation);
    }

    public void SetInitCOG(Vector3 _initCOG)
    {
        initCOG = _initCOG;
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
