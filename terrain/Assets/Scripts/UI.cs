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
    private UIConfig UIE; //UI elements
    private RobotConfig Robot;

    void Start()
    {
        UIE = this.gameObject.GetComponent<UIConfig>();
        SetupUIOptions();

        //setup toggle
        UIE.Toggle.onClick.AddListener(delegate { ToggleUI(); });
        //flip so that it will flip back to the default state
        UIE.IsCollapsed = !UIE.IsCollapsed;
        UIE.IsOriginal = !UIE.IsOriginal;
        ToggleUI();
    }

    private void SetupUIOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < Enum.GetNames(typeof(UIView)).Length; i++)
        {
            options.Add(new Dropdown.OptionData(Enum.GetName(typeof(UIView), i)));
        }
        UIE.UIOption.ClearOptions();
        UIE.UIOption.AddOptions(options);
        UIE.UIOption.onValueChanged.AddListener(delegate { SelectOption(UIE.View); });
        SelectOption(UIE.DefaultView);
    }

    private void SelectOption(UIView selection)
    {
        //disable other objects
        UIE.View = selection;
        switch (selection)
        {
            case UIView.Robot:
                ToggleEnable(UIE.PerformanceOptions, false);
                SetupRobotSelector();
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Performance:
                ToggleEnable(UIE.RobotOptions, false);
                if (!UIE.IsCollapsed)
                {               
                    ToggleEnable(UIE.PerformanceOptions, true);
                }
                UIE.RobotNumber.gameObject.SetActive(false);
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
        Text text = UIE.ToggleText;
        if(UIE.IsCollapsed)
        {
            //UI is currently down, needs to go up
            UIE.IsCollapsed = !UIE.IsCollapsed;
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, 440, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, 175, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, 440, 0);
            text.text = "▼";
            if (UIE.View == UIView.Robot)
            {
                UIE.RobotNumber.gameObject.SetActive(true);
                SelectRobot(UIE.RobotNumber.text);
                ToggleEnable(UIE.RobotOptions, true);
            }
        }
        else
        {
            //UI is currently up, needs to go down
            UIE.IsCollapsed = !UIE.IsCollapsed;
            ToggleEnable(UIE.RobotOptions, false);
            UIE.RobotNumber.gameObject.SetActive(false);
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, -400, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, -160, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, -400, 0);
            text.text = "▲";
        }
    }

    //------------------------------Robot Options--------------------------------
    private void SetupRobotSelector()
    {
        UIE.RobotNumber.gameObject.SetActive(true);
        UIE.RobotNumber.gameObject.transform.parent.gameObject.SetActive(true); //shouldn't be necessary but for some reason is
        UIE.RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(UIE.RobotNumber.text); });
        UIE.Original.onClick.AddListener(delegate { ToggleOriginal(); });
        SelectRobot(CameraConfig.CamFollow == -1 ? "1" : (CameraConfig.CamFollow + 1).ToString());
        if (!UIE.IsCollapsed) ToggleEnable(UIE.RobotOptions, true);
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
        UIE.RobotNumber.text = $"Robot {robot}";
        Robot = AIConfig.RobotConfigs[robot - 1];
        if (UIE.IsOriginal && !UIE.IsCollapsed) RobotUpdate(Robot.Original, false); //show original values
        else if (!UIE.IsCollapsed) RobotUpdate(Robot, false); //show current values

        //setup camera if not already done
        if(CameraConfig.CamFollow != Robot.RobotIndex)
        {
            Robot.AttachCam();
        }
    }

    private void RobotUpdate(RobotConfig config, bool ChangeColour)
    {
        //↶ ↷ ← symbols that will be used
        Text text;
        string original;

        //Version
        text = UIE.VersionText;
        original = text.text;
        text.text = Robot.Version.ToString();
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(UIE.Version, config.RobotIndex));

        //NoSections
        text = UIE.NoSectionsText;
        original = text.text;
        text.text = config.NoSections.Value.ToString();
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(UIE.NoSections, config.RobotIndex));

        //TailEnabled
        text = UIE.TailEnabledText;
        original = text.text;
        text.text = config.IsTailEnabled.Value ? "✓" : "";
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(UIE.TailEnabled, config.RobotIndex));

        //BodyColour
        text = UIE.BodyColourText;
        text.text = "■";
        Color originalColour = text.color;
        text.color = new Color(config.BodyColour.Value / 100f, config.BodyColour.Value / 100f, 1f);
        if (ChangeColour && text.color != originalColour) StartCoroutine(TextChanged(UIE.BodyColour, config.RobotIndex));
    }

    private void ToggleOriginal()
    {
        Text text = UIE.OriginalText;
        if (!UIE.IsOriginal)
        {
            //currently showing current, move to show original
            text.text = "Current";
            UIE.RobotOptions.ForEach(o => o.GetComponent<Image>().color = Color.grey);
            UIE.Original.GetComponent<Image>().color = Color.white; //return to white, exception to above line
            RobotUpdate(Robot.Original, false);
        }
        else
        {
            //currently showing original, move to show current
            text.text = "Original";
            UIE.RobotOptions.ForEach(o => o.GetComponent<Image>().color = Color.white);
            RobotUpdate(Robot, false);
        }
        UIE.IsOriginal = !UIE.IsOriginal;
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
            button.GetComponent<Image>().color = Color.white;
        }
    }

    //------------------------------------------Accessed by GA------------------------------

    public void UpdateRobotUI(RobotConfig config)
    {
        if (UIE.View == UIView.Robot && !UIE.IsOriginal && config.RobotIndex == Robot.RobotIndex) RobotUpdate(config, true);
    }

}
