using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodySettings : MonoBehaviour
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

    public int bodyConfigIndex = -1;

    void Start()
    {
        sectionsSlider.value = BaseConfig.DefaultNoSections;
        ChangeNoSections();

        defaultToggle.SetIsOnWithoutNotify(BaseConfig.IsDefault);
        ToggleDefault();
    }

    public void ChangeNoSections()
    {
        BaseConfig.NoSections = (int)sectionsSlider.value;
        if (BaseConfig.SectionConfigs.Count < BaseConfig.NoSections)
        {
            for (int i = BaseConfig.SectionConfigs.Count; i < BaseConfig.NoSections; i++)
            {
                //BodyConfig is initialised with default params
                BaseConfig.SectionConfigs.Add(new BodyConfig());
            }
        }
        else if (BaseConfig.SectionConfigs.Count > BaseConfig.NoSections)
        {
            for (int i = 0; i < BaseConfig.SectionConfigs.Count - BaseConfig.NoSections; i++)
            {
                BaseConfig.SectionConfigs.RemoveAt(BaseConfig.SectionConfigs.Count - 1);
            }
        }
        UIUpdateSectionsOutput();
        UIUpdateHiddenSettings();
    }

    public void UIUpdateSectionsOutput()
    {
        sectionsOutput.text = sectionsSlider.value.ToString();
    }

    public void ToggleDefault()
    {
        BaseConfig.IsDefault = defaultToggle.isOn;
        UIUpdateHiddenSettings();
    }

    public void UIUpdateHiddenSettings()
    {
        try
        {
            for (int i = 0; i < selections.Length; i++)
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
        for (int i = 0; i < selections.Length; i++)
        {
            ColorBlock cb = selections[i].colors;
            cb.normalColor = index == i + 1 ? Color.black : Color.white;
            selections[i].GetComponent<Image>().color = index == i + 1 ? Color.black : Color.white;
            selections[i].colors = cb;
        }
        UIUpdateRDToggles();
        configObject.GetComponent<SectionSettings>().UpdateIndex(index);
    }

    private void UIUpdateRDToggles()
    {
        BodyConfig config = BaseConfig.SectionConfigs[bodyConfigIndex - 1];
        SectionSettings objects = configObject.GetComponent<SectionSettings>();
        //have to use SetIsOnWithoutNotify because IsOn = fails when assigned false
        objects.RotateToggle.SetIsOnWithoutNotify(config.IsRotating);
        objects.DriveToggle.SetIsOnWithoutNotify(config.IsDriving);
    }
}
