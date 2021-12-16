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

    [SerializeField]
    private Button[] bodyPartButtons = new Button[3];

    private int bodyConfigIndex = -1;

    void Start()
    {
        //TODO: legs/tail update when implemented
        bodyPartButtons[1].interactable = false;
        bodyPartButtons[2].interactable = false;
        //default to body settings
        bodyPartButtons[0].Select();
        ShowRelevantSettings(0);
    }

    public void ShowRelevantSettings(int bodyPart)
    {

        switch ((BodyPart)bodyPart)
        {
            case BodyPart.Body:              
                ChangeNoSections();
                break;
            case BodyPart.Legs:

                break;
            case BodyPart.Tail:

                break;
        }
    }

    //when the generate robot button has been clicked 
    //pass the params - if not default then move to customisation
    //if default then move to the terrain scene
    public void GenerateRobot()
    {
        int terrainIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(terrainIndex);
    }

    public void ChangeNoSections()
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
        UIUpdateSectionsOutput();
        UIUpdateHiddenSettings();
    }

    private BodyConfig GenerateDefaultSectionConfig()
    {
        BodyConfig newConfig = new BodyConfig();
        newConfig.IsDriving = true;
        newConfig.IsRotating = false;
        return newConfig;
    }

    public void UIUpdateSectionsOutput()
    {
        sectionsOutput.text = sectionsSlider.value.ToString();
    }

    public void ToggleDefault()
    {
        BaseConfig.isDefault = defaultToggle.isOn;
        UIUpdateHiddenSettings();
    }

    public void UIUpdateHiddenSettings()
    {
        try
        {
            for(int i = 0; i < selections.Length; i++)
            {
                bool showButton = defaultToggle.isOn || (!defaultToggle.isOn && i + 1 > BaseConfig.NoSections) ? false : true;
                selections[i].interactable = showButton;
            }
            //update which one is selected
            if (!defaultToggle.isOn)
            {
                //is one already selected? default to number 1
                bodyConfigIndex = bodyConfigIndex == -1 ? 1 : 
                    bodyConfigIndex > BaseConfig.NoSections ? 1 : bodyConfigIndex; //is the selected one now hidden (reducing number of sections)
                selections[bodyConfigIndex - 1].Select();
                SelectSection(bodyConfigIndex);
            }
            //show section settings
            configObject.SetActive(!defaultToggle.isOn);
        }
        catch (System.Exception)
        {
            Debug.LogError("The selection buttons have been set up incorrectly");
        }
    }

    public void SelectSection(int index)
    {
        bodyConfigIndex = index;
        UIUpdateRDToggles();
    }

    private void UIUpdateRDToggles()
    {
        BodyConfig config = BaseConfig.SectionConfigs[bodyConfigIndex - 1];
        SectionSettingsUI objects = configObject.GetComponent<SectionSettingsUI>();
        objects.RotateToggle.isOn = config.IsRotating;
        objects.DriveToggle.isOn = config.IsDriving;
    }

    public void ToggleRotate()
    {

    }
}
