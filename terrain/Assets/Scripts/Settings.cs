using Config;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    private Text sectionsOutput;

    [SerializeField]
    private Slider sectionsSlider;

    [SerializeField]
    private Toggle defaultToggle;

    [SerializeField]
    private Button[] selections = new Button[10];

    [SerializeField]
    private GameObject configObject;

    private int bodyConfigIndex = -1;

    void Start()
    {
        UpdateNoSections();
    }

    //when the generate robot button has been clicked 
    //pass the params - if not default then move to customisation
    //if default then move to the terrain scene
    public void GenerateRobot()
    {
        int terrainIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(terrainIndex);
    }

    public void UpdateNoSections()
    {
        BaseConfig.NoSections = (int) sectionsSlider.value;
        if(BaseConfig.SectionConfigs.Count < BaseConfig.NoSections)
        {
            for(int i = BaseConfig.SectionConfigs.Count; i < BaseConfig.NoSections; i++)
            {
                BaseConfig.SectionConfigs.Add(GenerateDefaultSectionConfig());
            }
        }
        else if(BaseConfig.SectionConfigs.Count > BaseConfig.NoSections)
        {
            for (int i = 0; i < BaseConfig.SectionConfigs.Count - BaseConfig.NoSections; i++)
            {
                BaseConfig.SectionConfigs.RemoveAt(BaseConfig.SectionConfigs.Count - 1);
            }
        }
        UpdateSectionsOutput();
        UpdateSelectionButtons();
    }

    private BodyConfig GenerateDefaultSectionConfig()
    {
        BodyConfig newConfig = new BodyConfig();
        newConfig.IsDriving = true;
        newConfig.IsRotating = false;
        return newConfig;
    }

    public void UpdateSectionsOutput()
    {
        sectionsOutput.text = sectionsSlider.value.ToString();
    }

    public void UpdateDefault()
    {
        BaseConfig.isDefault = defaultToggle.isOn;
        UpdateSelectionButtons();
    }

    public void UpdateSelectionButtons()
    {
        try
        {
            for(int i = 0; i < selections.Length; i++)
            {
                bool showButton = defaultToggle.isOn || (!defaultToggle.isOn && i + 1 > BaseConfig.NoSections) ? false : true;
                selections[i].gameObject.SetActive(showButton);
            }
            //update which one is selected
            if (!defaultToggle.isOn)
            {
                //is one already selected? default to number 1
                bodyConfigIndex = bodyConfigIndex == -1 ? 1 : 
                    bodyConfigIndex > BaseConfig.NoSections ? 1 : bodyConfigIndex; //is the selected one now hidden (reducing number of sections)
                selections[bodyConfigIndex - 1].Select();
                UpdateSelectedSection(bodyConfigIndex);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("The selection buttons have been set up incorrectly");
        }
    }

    public void UpdateSelectedSection(int index)
    {
        bodyConfigIndex = index;
        UpdateRDToggles();
    }

    private void UpdateRDToggles()
    {
        BodyConfig config = BaseConfig.SectionConfigs[bodyConfigIndex - 1];
        SectionSettingsUI objects = configObject.GetComponent<SectionSettingsUI>();
        objects.RotateToggle.isOn = config.IsRotating;
        objects.DriveToggle.isOn = config.IsDriving;
    }
}
