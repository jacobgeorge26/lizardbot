using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject Head;
    public List<GameObject> Joints;
    public List<GameObject> Sections;
    bool coiling = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (coiling) CoilBody();
    }

    //iterate through each joint / section and rotate it
    //every other joint will not rotate to produce a wide coil
    //TO DO: make coil size dynamic
    //TO DO: make angle a parameter
    private void CoilBody()
    {
        double angle = Math.PI / 10;

        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints.ElementAt(index); //get current joint and section
            GameObject section = Sections.ElementAt(index);

            //rotate the joint
            joint.transform.Rotate(0, (float)angle, 0, Space.Self);

            //update the position of the coupled section to reflect this
            section.transform.Rotate(0, (float)angle, 0, Space.Self);
            section.transform.position = RotateAroundYAxis(joint, section, angle);
        }
        coiling = false; //testing - only do one rotation
    }

    private void adjustRemainingBody(int originalIndex, double angle)
    {
        for (int index = originalIndex + 1; index < Joints.Count; index++)
        {
            GameObject joint = Joints.ElementAt(index); //get current joint and section
            GameObject section = Sections.ElementAt(index);

            //rotate the joint
            joint.transform.Rotate(0, (float)angle, 0, Space.Self);

            //update the position of the coupled section to reflect this
            section.transform.Rotate(0, (float)angle, 0, Space.Self);
            section.transform.position = RotateAroundYAxis(joint, section, angle);
        }
    }

    //determine the new position of an object after being rotated x *radians* around the y axis
    private Vector3 RotateAroundYAxis(GameObject origin, GameObject point, double angle)
    {
        Vector3 translation = new Vector3(point.transform.position.x - origin.transform.position.x + (origin.transform.localScale.x), 0f, point.transform.position.z - origin.transform.position.z + (origin.transform.localScale.z));
        point.transform.position -= translation;
        //the position needs to be adjusted to the CENTRE of the joint
        //so pass that in
        float x = (float)((point.transform.position.x * Math.Cos(angle)) + (point.transform.position.z * Math.Sin(angle)));
        float y = point.transform.position.y;
        float z = (float)((point.transform.position.z * Math.Cos(angle)) - (point.transform.position.x * Math.Sin(angle)));
        point.transform.position += translation;
        return new Vector3(x, y, z);
    }
}
