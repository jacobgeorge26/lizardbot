using Config;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    private Button[] bodyPartButtons = new Button[3];

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
                this.GetComponent<BodySettings>().ChangeNoSections();
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
}
