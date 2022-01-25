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
    private bool IsEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        tail = GetComponent<Rigidbody>();
        config = this.gameObject.GetComponent<TailConfig>();
        objectConfig = this.gameObject.GetComponent<ObjectConfig>();
        robotConfig = AIConfig.RobotConfigs.Where(c => c.RobotIndex == objectConfig.RobotIndex).First();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsEnabled)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 cog = GetCOG();
        Vector3 bodyMomentum = GetMomentum(cog);
        float r = (tail.transform.position - cog).magnitude;
        Vector3 addVelocity = new Vector3();
        for (int i = 0; i < 3; i++)
        {

            float targetVelocity = bodyMomentum[i] / (r * tail.mass * -1);
            addVelocity[i] = targetVelocity - tail.velocity[i];
            //adjust for rotation multiplier
            addVelocity[i] *= config.RotationMultiplier.Value[i];
        }
        tail.AddForce(addVelocity, ForceMode.VelocityChange);
    }

    private Vector3 GetMomentum(Vector3 cog)
    {
        Vector3 momentum = new Vector3();
        //l = rmv
        foreach (ObjectConfig objConfig in robotConfig.Configs.Where(o => o.Type != BodyPart.Tail))
        {
            GameObject obj = objConfig.Object;
            Rigidbody objRigidBody = obj.GetComponent<Rigidbody>();
            float m = objRigidBody.mass;
            float r = (obj.transform.position - cog).magnitude;
            Vector3 v = objRigidBody.velocity;
            for (int i = 0; i < 3; i++)
            {
                momentum[i] += r * m * v[i];
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
            GameObject obj = objConfig.Object;
            Rigidbody sectionRigidBody = obj.GetComponent<Rigidbody>();
            Vector3 position = obj.transform.position + sectionRigidBody.centerOfMass;
            float mass = sectionRigidBody.mass;
            sumMass += mass;
            for (int i = 0; i < 2; i++)
            {
                sumMassByPos[i] += mass * position[i];
            }
        }
        return sumMassByPos / sumMass;
    }

    internal void Enable()
    {
        IsEnabled = true;
    }
}
