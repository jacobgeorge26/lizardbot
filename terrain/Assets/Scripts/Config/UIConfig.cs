using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConfig : MonoBehaviour
{
    //Important - how static scripts will access the UI (aka GA to update robot)
    public static GameObject UIContainer;

    //UI options
    public static bool IsUIEnabled = true;
    //Defaults
    public UIView DefaultView = UIView.Performance;
    public bool IsCollapsed;
    [HideInInspector]
    public bool IsOriginal = false;

    //BOTH
    public GameObject panel;
    public Dropdown UIOption;
    public Button Toggle;
    //Helpers for both
    [HideInInspector]
    public Text ToggleText;
    [HideInInspector]
    public UIView View
    {
        get => (UIView)UIOption.value;
        set => UIOption.value = (int)value;
    }

    //ROBOT
    public InputField RobotNumber;
    public Button Original;
    public Button Version;
    public Button CurrentPerformance;
    public Button BestPerformance;

    //Text objects for robot display - save repeated calls for GetComponent
    [HideInInspector]
    public Text OriginalText;
    [HideInInspector]
    public Text VersionText;
    [HideInInspector]
    public Text CurrentPerformanceText;
    [HideInInspector]
    public Text BestPerformanceText;

    [HideInInspector]
    public List<GameObject> RobotOptions, PerformanceOptions;

    [HideInInspector]
    public List<UITemplateBody> Bodies;

    void Awake()
    {
        UIContainer = this.gameObject;

        //split objects into Robot / Performance
        //Robot
        RobotOptions = new List<GameObject>();
        RobotOptions.Add(Original.gameObject);
        RobotOptions.Add(Version.gameObject);
        RobotOptions.Add(CurrentPerformance.gameObject);
        RobotOptions.Add(BestPerformance.gameObject);
        //Performance
        PerformanceOptions = new List<GameObject>();
        //TODO: add performance UI

        //setup text objects
        ToggleText = Toggle.GetComponentInChildren<Text>();
        OriginalText = Original.GetComponentInChildren<Text>();
        VersionText = Version.GetComponentInChildren<Text>();
        CurrentPerformanceText = CurrentPerformance.GetComponentInChildren<Text>();
        BestPerformanceText = BestPerformance.GetComponentInChildren<Text>();
    }
}


