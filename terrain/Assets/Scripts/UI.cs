using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    //BOTH
    public GameObject panel;
    public Dropdown UIOption;
    public Button Toggle;

    //ROBOT
    public InputField RobotNumber;
    public Button Original;
    public Button Version;
    public Button NoSections;
    public Button TailEnabled;
    public Button BodyColour;

    private RobotConfig Robot;
    private UIView DefaultView = UIView.Performance;
    private bool IsCollapsed, IsOriginal;

    private List<GameObject> RobotOptions = new List<GameObject>();
    private List<GameObject> PerformanceOptions = new List<GameObject>();

    void Start()
    {
        SetupUIOptions();

        //split objects into Robot / Performance
        RobotOptions.Add(Original.gameObject);
        RobotOptions.Add(Version.gameObject);
        RobotOptions.Add(NoSections.gameObject);
        RobotOptions.Add(TailEnabled.gameObject);
        RobotOptions.Add(BodyColour.gameObject);

        //setup toggle
        Toggle.onClick.AddListener(delegate { ToggleUI(); });
        IsCollapsed = false;
        ToggleUI();

        //TODO: add performance options
    }

    private void SetupUIOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < Enum.GetNames(typeof(UIView)).Length; i++)
        {
            options.Add(new Dropdown.OptionData(Enum.GetName(typeof(UIView), i)));
        }
        UIOption.ClearOptions();
        UIOption.AddOptions(options);
        UIOption.onValueChanged.AddListener(delegate { SelectOption((UIView)UIOption.value); });
        SelectOption(DefaultView);
    }

    private void SelectOption(UIView selection)
    {
        //disable other objects
        UIOption.value = (int)selection;
        switch (selection)
        {
            case UIView.Robot:
                ToggleEnable(PerformanceOptions, false);
                SetupRobotSelector();
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Performance:
                ToggleEnable(RobotOptions, false);
                if (!IsCollapsed)
                {               
                    ToggleEnable(PerformanceOptions, true);
                }
                RobotNumber.gameObject.SetActive(false);
                CameraConfig.OverviewCamera.SetActive(true);
                CameraConfig.RobotCamera.SetActive(false);
                CameraConfig.Hat.SetActive(false);
                break;
            default:
                break;
        }
    }

    //--------------------------------------------Toggle-------------------------------------
    private void ToggleUI()
    {
        Text text = Toggle.GetComponentInChildren<Text>();
        if(IsCollapsed)
        {
            //UI is currently down, needs to go up
            IsCollapsed = !IsCollapsed;
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            UIOption.transform.localPosition = new Vector3(-800, 440, 0);
            RobotNumber.transform.localPosition = new Vector3(-525, 175, 0);
            Toggle.transform.localPosition = new Vector3(920, 440, 0);
            text.text = "▼";
            if ((UIView)UIOption.value == UIView.Robot)
            {
                RobotNumber.gameObject.SetActive(true);
                SelectRobot(RobotNumber.text);
                ToggleEnable(RobotOptions, true);
            }
        }
        else
        {
            //UI is currently up, needs to go down
            IsCollapsed = !IsCollapsed;
            ToggleEnable(RobotOptions, false);
            RobotNumber.gameObject.SetActive(false);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIOption.transform.localPosition = new Vector3(-800, -400, 0);
            RobotNumber.transform.localPosition = new Vector3(-525, -160, 0);
            Toggle.transform.localPosition = new Vector3(920, -400, 0);
            text.text = "▲";
        }
    }

    //------------------------------Robot Options--------------------------------
    private void SetupRobotSelector()
    {
        RobotNumber.gameObject.SetActive(true);
        RobotNumber.gameObject.transform.parent.gameObject.SetActive(true); //shouldn't be necessary but for some reason is
        RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(RobotNumber.text); });
        Original.onClick.AddListener(delegate { ToggleOriginal(); });
        SelectRobot(CameraConfig.CamFollow == -1 ? "1" : (CameraConfig.CamFollow + 1).ToString());
        if (!IsCollapsed) ToggleEnable(RobotOptions, true);
    }

    private void SelectRobot(string robotText)
    {
        robotText = Regex.Match(robotText, @"\d+").Value;
        int robot = 0;
        try
        {
            robot = Int32.Parse(robotText);
        }
        catch (FormatException)
        {
            //robot will default to the first one
        }
        robot = robot < 1 || robot > AIConfig.PopulationSize ? 1 : robot;
        RobotNumber.text = $"Robot {robot}";
        Robot = AIConfig.RobotConfigs[robot - 1];
        if (IsOriginal && !IsCollapsed) RobotUpdate(Robot.Original, false); //show original values
        else if (!IsCollapsed) RobotUpdate(Robot, false); //show current values

        //setup camera if not already done
        if(CameraConfig.CamFollow != Robot.RobotIndex)
        {
            RobotHelpers helpers = Robot.Object.GetComponent<RobotHelpers>();
            helpers.AttachCam();
        }
    }

    private void RobotUpdate(RobotConfig config, bool ChangeColour)
    {
        //↶ ↷ ← symbols that will be used
        Text text;
        string original;

        //Version
        text = Version.GetComponentInChildren<Text>();
        original = text.text;
        text.text = Robot.Version.ToString();
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(Version, config.RobotIndex));

        //NoSections
        text = NoSections.GetComponentInChildren<Text>();
        original = text.text;
        text.text = config.NoSections.Value.ToString();
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(NoSections, config.RobotIndex));

        //TailEnabled
        text = TailEnabled.GetComponentInChildren<Text>();
        original = text.text;
        text.text = config.IsTailEnabled.Value ? "✓" : "";
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(TailEnabled, config.RobotIndex));

        //BodyColour
        text = BodyColour.GetComponentInChildren<Text>();
        text.text = "■";
        Color originalColour = text.color;
        text.color = new Color(config.BodyColour.Value / 100f, config.BodyColour.Value / 100f, 1f);
        if (ChangeColour && text.color != originalColour) StartCoroutine(TextChanged(BodyColour, config.RobotIndex));
    }

    private void ToggleOriginal()
    {
        Text text = Original.GetComponentInChildren<Text>();
        if (IsOriginal)
        {
            //currently showing original, move to show current
            text.text = "Original";
            RobotOptions.ForEach(o => o.GetComponent<Image>().color = Color.white);
            RobotUpdate(Robot, false);
        }
        else
        {
            //currently showing current, move to show original
            text.text = "Current";
            RobotOptions.ForEach(o => o.GetComponent<Image>().color = Color.grey);
            Original.GetComponent<Image>().color = Color.white; //return to white, exception to above line
            RobotUpdate(Robot.Original, false);
        }
        IsOriginal = !IsOriginal;
    }

    //-----------------------------------------------All Helpers----------------------------
    private void ToggleEnable(List<GameObject> objects, bool enable)
    {
        foreach (var item in objects)
        {
            item.SetActive(enable);
            item.transform.parent.gameObject.SetActive(enable); //the label associated with these values
        }
    }

    private IEnumerator TextChanged(Button button, int robotIndex)
    {
        button.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            button.GetComponentInChildren<Text>().text = "";
            button.GetComponent<Image>().color = Color.white;
        }
    }

    //------------------------------------------Accessed by GA------------------------------

    public void UpdateRobotUI(RobotConfig config)
    {
        if ((UIView)UIOption.value == UIView.Robot && !IsOriginal && config.RobotIndex == Robot.RobotIndex) RobotUpdate(config, true);
    }

}
