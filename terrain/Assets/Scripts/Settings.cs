using Config;
using UnityEngine;
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

    void Start()
    {
        BaseConfig.NoSections = (int)sectionsSlider.value;
        UpdateSectionsOutput();
        UpdateSelectionButtons();
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
        UpdateSectionsOutput();
        UpdateSelectionButtons();
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
        }
        catch (System.Exception)
        {
            Debug.LogError("The selection buttons have been set up incorrectly");
        }
    }


}
