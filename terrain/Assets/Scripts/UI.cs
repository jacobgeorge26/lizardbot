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
    public GameObject panel;
    public Dropdown UIOption;
    public Button Toggle;
    public InputField RobotNumber;
    public List<Button> NoSections;
    public List<Button> TailEnabled;
    public List<Button> BodyColour;

    private RobotConfig Robot;
    private UIView DefaultView = UIView.Performance;
    private bool IsCollapsed;

    private List<GameObject> RobotOptions = new List<GameObject>();
    private List<GameObject> PerformanceOptions = new List<GameObject>();

    void Start()
    {
        SetupUIOptions();

        //split objects into Robot / Performance
        NoSections.ForEach(o => RobotOptions.Add(o.gameObject));
        TailEnabled.ForEach(o => RobotOptions.Add(o.gameObject));
        BodyColour.ForEach(o => RobotOptions.Add(o.gameObject));

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
                if (!IsCollapsed)
                {
                    ToggleEnable(PerformanceOptions, false);
                }
                SetupRobotSelector();
                this.transform.SetParent(CameraConfig.RobotCamera.transform);
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Performance:
                if (!IsCollapsed)
                {
                    ToggleEnable(RobotOptions, false);
                    RobotNumber.gameObject.SetActive(false);
                    ToggleEnable(PerformanceOptions, true);
                    //TODO: Performance UI
                }
                this.transform.SetParent(CameraConfig.OverviewCamera.transform);
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
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            UIOption.transform.localPosition = new Vector3(-750, 335, 0);
            RobotNumber.transform.localPosition = new Vector3(-450, 110, 0);
            Toggle.transform.localPosition = new Vector3(910, 335, 0);
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
            ToggleEnable(RobotOptions, false);
            RobotNumber.gameObject.SetActive(false);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIOption.transform.localPosition = new Vector3(-750, -400, 0);
            RobotNumber.transform.localPosition = new Vector3(-450, -132, 0);
            Toggle.transform.localPosition = new Vector3(910, -400, 0);
            text.text = "▲";
        }
        IsCollapsed = !IsCollapsed;
    }

    //------------------------------Robot Options--------------------------------
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
        RobotUpdate((int)UIRobotType.Original, Robot.Original); //show original values
        NoSections[1].GetComponentInChildren<Text>().text = "";
        NoSections[2].GetComponentInChildren<Text>().text = "";

        //setup camera if not already done
        if(CameraConfig.CamFollow != Robot.RobotIndex)
        {
            RobotHelpers helpers = Robot.gameObject.GetComponent<RobotHelpers>();
            helpers.AttachCam();
        }
    }

    private void SetupRobotSelector()
    {
        RobotNumber.gameObject.SetActive(true);
        RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(RobotNumber.text); });
        SelectRobot(CameraConfig.CamFollow == -1 ? "1" : (CameraConfig.CamFollow + 1).ToString());
        if(!IsCollapsed) ToggleEnable(RobotOptions, true);
    }

    private void RobotUpdate(int index, RobotConfig config)
    {
        //NoSections
        Text text = NoSections[index].GetComponentInChildren<Text>();
        text.text = config.NoSections.Value.ToString();
        //TailEnabled
        text = TailEnabled[index].GetComponentInChildren<Text>();
        text.text = config.IsTailEnabled.Value ? "✓" : "";
        //BodyColour
        text = BodyColour[index].GetComponentInChildren<Text>();
        text.text = "■";
        text.color = new Color(config.BodyColour.Value / 100f, config.BodyColour.Value / 100f, 1f);
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

    //------------------------------------------Accessed by GA------------------------------

    public void UpdateUI(int index, RobotConfig config)
    {
        if ((UIView)UIOption.value == UIView.Robot && config.RobotIndex == Robot.RobotIndex) RobotUpdate(index, config);
    }

}
