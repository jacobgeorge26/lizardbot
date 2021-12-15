using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    //when the generate robot button has been clicked 
    //pass the params - if not default then move to customisation
    //if default then move to the terrain scene
    public void GenerateRobot()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GenerateRobot controller = FindObjectOfType<GenerateRobot>();
        
    }


}
