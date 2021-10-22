using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ------------------------
 * This is the script for:
 * when I was exploring the option of configurable joints, testing the functionality
 * ------------------------
 */

//public class TestJointSetup : MonoBehaviour
//{
//    Rigidbody m_Rigidbody;
//    Vector3 m_EulerAngleVelocity;
//    // Start is called before the first frame update
//    void Start()
//    {
//        //Fetch the Rigidbody from the GameObject with this script attached
//        m_Rigidbody = GetComponent<Rigidbody>();

//        //Set the angular velocity of the Rigidbody (rotating around the Y axis, 100 deg/sec)
//        m_EulerAngleVelocity = new Vector3(0, -100, 0);
//    }

//    private void FixedUpdate()
//    {
//        if (this.transform.localRotation.eulerAngles.y > -30)
//        {
//            Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
//            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
//            Debug.Log(this.transform.localRotation.eulerAngles);
//        }

//    }

//    //// Update is called once per frame
//    //void Update()
//    //{
//    //    //StartCoroutine(RotateJoint(30));
//    //    var startRotation = transform.localRotation;
//    //    var myJoint = GetComponent<ConfigurableJoint>();
//    //    myJoint.targetRotation = Quaternion.Euler(0, 60, 0);
//    //    //myJoint.SetTargetRotationLocal(Quaternion.Euler(0, 30, 0), startRotation);
//    //}

//    private IEnumerator RotateJoint(double targetAngle)
//    {
//        float moveSpeed = 0.00005f;
//        Vector3 currentAngle = this.transform.localEulerAngles;
//        while ((targetAngle > 0 && currentAngle.y < targetAngle) || (targetAngle < 0 && currentAngle.y > targetAngle) || (targetAngle == 0 && currentAngle.y != 0))
//        {
//            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, Quaternion.Euler(0, (float)targetAngle, 0), moveSpeed);
//            yield return null;
//            //if (state != MoveState.Rotate) yield break;
//        }
//    }
//}

//public static class ConfigurableJointExtensions
//{
//    /// <summary>
//    /// Sets a joint's targetRotation to match a given local rotation.
//    /// The joint transform's local rotation must be cached on Start and passed into this method.
//    /// </summary>
//    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
//    {
//        if (joint.configuredInWorldSpace)
//        {
//            Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
//        }
//        SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
//    }

//    static void SetTargetRotationInternal (this ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
//     {
//         // Calculate the rotation expressed by the joint's axis and secondary axis
//         var right = joint.axis;
//         var forward = Vector3.Cross (joint.axis, joint.secondaryAxis).normalized;
//         var up = Vector3.Cross (forward, right).normalized;
//         Quaternion worldToJointSpace = Quaternion.LookRotation (forward, up);
         
//         // Transform into world space
//         Quaternion resultRotation = Quaternion.Inverse (worldToJointSpace);
         
//         // Counter-rotate and apply the new local rotation.
//         // Joint space is the inverse of world space, so we need to invert our value
//         if (space == Space.World) {
//             resultRotation *= startRotation * Quaternion.Inverse (targetRotation);
//         } else {
//             resultRotation *= Quaternion.Inverse (targetRotation) * startRotation;
//         }
         
//         // Transform back into joint space
//         resultRotation *= worldToJointSpace;
         
//         // Set target rotation to our newly calculated rotation
//         joint.targetRotation = resultRotation;
//     }
//}
