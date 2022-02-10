using UnityEngine;
public class ColourTest : MonoBehaviour
{
    void Start()
    {
        GameObject sphere = this.gameObject;
        //Get the Renderer component from the new cube
        var cubeRenderer = sphere.GetComponent<Renderer>();

        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", new Color(150, 0, 0));
    }
}