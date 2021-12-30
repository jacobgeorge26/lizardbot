using Config;
using System;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSettings : MonoBehaviour
{
    [SerializeField]
    public Toggle RotateToggle;

    [SerializeField]
    public Toggle DriveToggle;

    [SerializeField]
    private GameObject rotateSettings;

    [SerializeField]
    private GameObject driveSettings;

    //drive velocity
    [SerializeField]
    private Slider driveVelocitySlider;

    [SerializeField]
    private Text driveVelocityOutput;

    [SerializeField]
    private Slider[] angleConstraintSliders = new Slider[3];

    [SerializeField]
    private Text[] angleConstraintOutput = new Text[3];

    [SerializeField]
    private Slider[] turnRatioSliders = new Slider[3];

    [SerializeField]
    private Text[] turnRatioOutputs = new Text[3];

    private int sectionIndex = -1;

    private BodyConfig config;


    void Start()
    {
        GenerateDefaultParams();
    }

    public void UpdateIndex(int index)
    {
        sectionIndex = index;
        config = BaseConfig.SectionConfigs[sectionIndex - 1];
        GenerateDefaultParams();
    }
    private void GenerateDefaultParams()
    {
        //BodyConfig is initialised with default values
        //this method leads to a small amount of repetition as config will be updated with the same values
        //however calling the methods is a simple way to ensure every component is updated correctly - changes to the code won't lead to some things being missed

        //isRotating
        RotateToggle.isOn = config.IsRotating;
        ToggleRotate();

        //isDriving
        DriveToggle.isOn = config.IsDriving;
        ToggleDrive();

        //drive velocity
        driveVelocitySlider.value = config.DriveVelocity;
        ChangeDriveVelocity();

        //max angle
        for (int i = 0; i < angleConstraintOutput.Length; i++)
        {
            angleConstraintSliders[i].value = config.AngleConstraint[i];
            ChangeAngleConstraint(i);
        }

        //turn ratio
        for (int i = 0; i < turnRatioOutputs.Length; i++)
        {
            turnRatioSliders[i].value = config.RotationMultiplier[i];
            ChangeTurnRatio(i);
        }
    }

    public void ToggleRotate()
    {
        rotateSettings.gameObject.SetActive(RotateToggle.isOn);
        config.IsRotating = RotateToggle.isOn;
    }


    public void ToggleDrive()
    {
        driveSettings.gameObject.SetActive(DriveToggle.isOn);
        config.IsDriving = DriveToggle.isOn;
    }


    public void ChangeDriveVelocity()
    {
        driveVelocityOutput.text = driveVelocitySlider.value.ToString("N2");
        config.DriveVelocity = driveVelocitySlider.value;
    }

    public void ChangeAngleConstraint(int index)
    {
        angleConstraintOutput[index].text = angleConstraintSliders[index].value.ToString();
        Vector3 newAngleConstraint = config.AngleConstraint;
        newAngleConstraint[index] = angleConstraintSliders[index].value;
        config.AngleConstraint = newAngleConstraint;      
    }

    public void ChangeTurnRatio(int index)
    {
        turnRatioOutputs[index].text = turnRatioSliders[index].value.ToString();
        Vector3 newTurnRatio = config.RotationMultiplier;
        newTurnRatio[index] = turnRatioSliders[index].value;
        config.RotationMultiplier = newTurnRatio;
    }


}