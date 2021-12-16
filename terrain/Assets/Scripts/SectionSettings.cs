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

    //turn velocity
    [SerializeField]
    private Slider turnVelocitySlider;

    [SerializeField]
    private Text turnVelocityOutput;

    private int sectionIndex = -1;

    private BodyConfig config;

    void Start()
    {
        GenerateDefaultParams();
        //driveVelocityOutput.text = config != null && config.DriveVelocity >= 0 ? config.DriveVelocity.ToString("N2") : driveVelocitySlider.value.ToString("N2");
        //turnVelocityOutput.text = config != null && config.TurnVelocity >= 0 ? config.TurnVelocity.ToString() : turnVelocitySlider.value.ToString();
    }

    public void UpdateIndex(int index)
    {
        sectionIndex = index;
        config = BaseConfig.SectionConfigs[sectionIndex - 1];
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

        //turn velocity
        turnVelocitySlider.value = config.TurnVelocity;
        ChangeTurnVelocity();

        //drive velocity
        driveVelocitySlider.value = config.DriveVelocity;
        ChangeDriveVelocity();
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

    public void ChangeTurnVelocity()
    {
        turnVelocityOutput.text = turnVelocitySlider.value.ToString();
        config.TurnVelocity = (int)turnVelocitySlider.value;
    }


}