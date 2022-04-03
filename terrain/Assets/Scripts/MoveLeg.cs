using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MoveLeg : MonoBehaviour
{
    private RobotConfig robot;
    private Rigidbody leg;
    private LegConfig config;
    private ObjectConfig objectConfig;
    private GameObject body;

    static int circleResolution = 12;
    Vector3[] circle;

    void Start()
    {
        leg = GetComponent<Rigidbody>();
        try
        {
            objectConfig = this.gameObject.GetComponent<ObjectConfig>();
            config = objectConfig.Leg;
        }
        catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return; }

        try { robot = AIConfig.RobotConfigs.Where(c => c.RobotIndex == objectConfig.RobotIndex).First(); }
        catch (Exception ex) { GameController.Controller.TotalRespawn(ex.ToString()); return; }

        try { body = robot.Configs.First(o => o.Type == BodyPart.Body && o.Index == config.AttachedBody).gameObject; }
        catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), robot); return; }
        
        SetupCircle();
    }

    void FixedUpdate()
    {
        if (robot.Object == null) this.enabled = false;
        else if (robot.IsEnabled && circle != null)
        {
            Move();
        }
    }

    private void SetupCircle()
    {
        circle = new Vector3[circleResolution];

        //get origin point to left/right of body depending on spawn point
        Vector3 sideOfBody = config.Position == LegPosition.Right ? body.transform.right : body.transform.right * -1;
        Vector3 D = body.transform.localPosition + sideOfBody * config.Length.Value / 2;

        //get two points at 90* to each other from the origin point
        Vector3 A = D + new Vector3(0, 1, 0);
        Vector3 B = D + new Vector3(0, 0, 1);

        //get vectors from origin to each, with the desired radius of the offset axis circle
        float theta = (float)(Math.PI / 180f * config.AngleOffset.Value);
        float r = 10 * Mathf.Tan(theta) * config.Length.Value / 2;
        Vector3 V = (A - D) * r;
        Vector3 U = (B - D) * r;

        //this equation was taken from https://math.stackexchange.com/questions/3007243/plot-points-around-circumference-of-circle-in-3d-space-given-3-points
        //P = D + Vcos0 + Usin0
        for (int i = 0; i < circleResolution; i++)
        {
            theta = (float)(Math.PI / 180f * i * (360 / circleResolution));
            Vector3 P = (V * Mathf.Cos(theta)) + (U * Mathf.Sin(theta));
            circle[i] = P;
        }
    }

    private void Move()
    {
        //get origin point to left/right of body depending on spawn point
        Vector3 sideOfBody = config.Position == LegPosition.Right ? body.transform.right : body.transform.right * -1;
        Vector3 D = body.transform.localPosition + sideOfBody * config.Length.Value / 2;

        //get point in circle closest to where the leg is currently positioned
        Vector3 closestPoint = circle.OrderBy(p => Vector3.Angle(p, leg.transform.localPosition - D)).First();
        int index = circle.ToList().IndexOf(closestPoint);

        //get the next point in the circle to essentially create a tangent of the circle
        int nextIndex = index == circle.Length - 1 ? 0 : index + 1;
        Vector3 nextPoint = circle[nextIndex];

        //take into account how much force should be applied and deduct the existing force
        float force = 0.1f;
        Vector3 targetVelocity = (nextPoint - closestPoint) * force;
        Vector3 addVelocity = targetVelocity - leg.velocity;

        Rigidbody attachedBody = body.GetComponent<Rigidbody>();
        Vector3 bodyForward = body.transform.forward;
        Vector3 bodyVelocity = attachedBody.velocity;

        if (robot.MaintainGait.Value)
        {
            //get the direction of the body - is it rotating left or right?
            //these two lines of code are taken from this stackoverflow page
            //https://stackoverflow.com/questions/65794490/unity3d-check-if-a-point-is-to-the-left-or-right-of-a-vector
            Vector3 delta = (body.transform.localPosition + bodyVelocity - body.transform.localPosition).normalized;
            Vector3 cross = Vector3.Cross(delta, body.transform.localPosition + bodyForward);

            double direction = Math.Round(cross.y); //>0 then right, <0 then left

            //if body is moving right and leg is on the left then increase velocity
            //if body is moving left and leg is on the right then increase velocity
            addVelocity *= direction > 0
                ? (config.Position == LegPosition.Right ? 1 / config.GaitMultiplier.Value : config.GaitMultiplier.Value) //body is moving right - what about the leg position?
                : (config.Position == LegPosition.Right ? config.GaitMultiplier.Value : 1 / config.GaitMultiplier.Value); //body is moving left - what about the leg position?            
        }

        //add in rotation multiplier and avoid extreme velocity either way
        for (int i = 0; i < 3; i++)
        {
            addVelocity[i] *= config.RotationMultiplier.Value[i];
            addVelocity[i] = Math.Min(addVelocity[i], 100);
            addVelocity[i] = addVelocity[i] == 0 ? 0.01f : addVelocity[i];
        }

        leg.AddForce(addVelocity, ForceMode.VelocityChange);
    }

}
