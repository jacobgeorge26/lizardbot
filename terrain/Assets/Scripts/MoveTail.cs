using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Config;

public class MoveTail : MonoBehaviour
{
    private RobotConfig robotConfig;
    private Rigidbody tail;
    private TailConfig config;
    private ObjectConfig objectConfig;
    // Start is called before the first frame update
    void Start()
    {
        tail = GetComponent<Rigidbody>();
        objectConfig = this.gameObject.GetComponent<ObjectConfig>();
        config = objectConfig.Tail;
        robotConfig = AIConfig.RobotConfigs.Where(c => c.RobotIndex == objectConfig.RobotIndex).First();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (robotConfig.IsEnabled)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 cog = GetCOG();
        Vector3 bodyMomentum = GetMomentum(cog);
        float r = (tail.transform.position + tail.centerOfMass - cog).magnitude;
        Vector3 addVelocity = new Vector3();
        Vector3 targetVelocity = new Vector3();
        for (int i = 0; i < 3; i++)
        {          
            targetVelocity[i] = bodyMomentum[i] / (r * tail.mass * -1);
            addVelocity[i] = targetVelocity[i] - tail.velocity[i];
            //adjust for rotation multiplier
            addVelocity[i] *= config.RotationMultiplier.Value[i];
        }
        tail.AddForce(addVelocity, ForceMode.VelocityChange);
    }

    private Vector3 GetMomentum(Vector3 cog)
    {
        Vector3 momentum = new Vector3();
        //l = rmv
        foreach (ObjectConfig objConfig in robotConfig.Configs)
        {
            GameObject obj = objConfig.gameObject;
            Rigidbody objRigidBody = obj.GetComponent<Rigidbody>();
            float m = objRigidBody.mass;
            float r = (obj.transform.position + objRigidBody.centerOfMass - cog).magnitude;
            Vector3 v = objRigidBody.velocity;
            Vector3 p = new Vector3();
            for (int i = 0; i < 3; i++)
            {
                p[i] = r * m * v[i];
                momentum[i] += p[i];
            }
        }
        return momentum;
    }

    private Vector3 GetCOG()
    {
        Vector3 sumMassByPos = new Vector3();
        float sumMass = 0;
        //iterate objects, get sum of (mass * axis coordinate), divide by sum of all masses
        foreach(ObjectConfig objConfig in robotConfig.Configs)
        {
            GameObject obj = objConfig.gameObject;
            Rigidbody sectionRigidBody = obj.GetComponent<Rigidbody>();
            Vector3 position = obj.transform.position + sectionRigidBody.centerOfMass;
            float mass = sectionRigidBody.mass;
            sumMass += mass;
            for (int i = 0; i < 3; i++)
            {
                sumMassByPos[i] += mass * position[i];
            }
        }
        return sumMassByPos / sumMass;
    }
}
