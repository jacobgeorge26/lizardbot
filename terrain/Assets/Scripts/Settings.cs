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

    void Start()
    {
        sectionsOutput.text = sectionsSlider.value.ToString();
    }

    //when the generate robot button has been clicked 
    //pass the params - if not default then move to customisation
    //if default then move to the terrain scene
    public void GenerateRobot()
    {
        BaseConfig.isDefault = true;
        int terrainIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(terrainIndex);
    }

    public void UpdateNoSections(Slider slider)
    {
        BaseConfig.NoSections = (int) slider.value;
        sectionsOutput.text = slider.value.ToString();
    }


}
