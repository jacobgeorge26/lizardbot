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
        bool increase = true;
        double angle = Math.PI / 10;

        for (int index = 0; index < Joints.Count; index++)
        {
            GameObject joint = Joints.ElementAt(index); //get current joint and section
            GameObject section = Sections.ElementAt(index);

            //if they aren't the ones immediately after the head then they need to update their position
            //this is to reflect from the previous iterations rotation
            if (index > 0)
            {
                joint.transform.localPosition = RotateAroundYAxis(joint.transform.position, angle);
                section.transform.localPosition = RotateAroundYAxis(section.transform.position, angle);
            }

            //rotate to coil
            if (index % 2 == 0) //for wide coil
            {
                //determine if the coil is going clockwise or anticlockwise
                angle = increase ? angle : -1 * angle;
                increase = !increase;

                //rotate the joint
                joint.transform.Rotate(0, (float)angle, 0, Space.Self);
                
                //update the position of the coupled section to reflect this
                section.transform.localPosition = RotateAroundYAxis(section.transform.position, angle);
                section.transform.Rotate(0, (float)angle, 0, Space.Self);
            }
        }
        //coiling = Math.Abs(Joints[0].transform.localRotation.y) < Math.PI / 6;
        coiling = false; //testing - only do one rotation
    }

    //determine the new position of an object after being rotated x *radians* around the y axis
    private Vector3 RotateAroundYAxis(Vector3 position, double angle)
    {
        float x = (float)((position.x * Math.Cos(angle)) + (position.z * Math.Sin(angle)));
        float y = position.y;
        float z = (float)((position.z * Math.Cos(angle)) - (position.x * Math.Sin(angle)));
        return new Vector3(x, y, z);
    }
}
