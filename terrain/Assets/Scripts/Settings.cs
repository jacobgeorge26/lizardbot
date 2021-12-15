using Config;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Config Object")]
    protected GameObject config;
    //when the generate robot button has been clicked 
    //pass the params - if not default then move to customisation
    //if default then move to the terrain scene
    public void GenerateRobot()
    {
        BaseConfig.isDefault = true;
        int terrainIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(terrainIndex);

        //GameObject controller = GameObject.FindGameObjectWithTag("hello");
        //if(controller == null)
        //{
        //    controller = GameObject.Find("Controller");
        //}
        //if(controller == null)
        //{
        //    controller = GameObject.FindWithTag("hello");
        //}
        //GenerateRobot robot = controller.GetComponent<GenerateRobot>();
        //robot.UpdateParams(config.GetComponent<BaseConfig>());
    }


}
