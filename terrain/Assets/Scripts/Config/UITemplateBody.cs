using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITemplateBody : MonoBehaviour
{
    public Button Body;
    public Text PrimaryRotation;
    public Text IsRotating;
    public Text IsDriving;
    public Text DriveVelocity;
    public GameObject SizeMassChanged;
    //the UI needs to know the relative angle of the Body (for AngleConstraint), rather than the actual y coordinate
    [HideInInspector]
    public float RelativeAngle = 0;
    //the UI needs to know the relative scale of the Body (for Size), rather than the actual localScale as this is always returning 1
    [HideInInspector]
    public float RelativeScale = 1f;
}
