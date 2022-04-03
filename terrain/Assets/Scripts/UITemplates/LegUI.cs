using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegUI : MonoBehaviour
{
    public Button Leg;
    public Text Offset;
    public Text GaitMultiplier;
    public GameObject MassLengthChanged;
    //the UI needs to know the relative scale of the Leg (for Size), rather than the actual localScale as this is always returning 1
    [HideInInspector]
    public float RelativeScale = 1f;
}
