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
    public Dropdown UIOption;
    public InputField RobotNumber;
    public List<Button> NoSections;

    private RobotConfig Robot;
    private UIView DefaultView = UIView.Performance;

    private List<GameObject> RobotOptions = new List<GameObject>();

    void Start()
    {
        SetupUIOptions();
        NoSections.ForEach(o => RobotOptions.Add(o.gameObject));
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
                SetupRobotSelector();
                break;
            case UIView.AI:
                break;
            case UIView.Performance:
                break;
            default:
                break;
        }
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
    }

    private void SetupRobotSelector()
    {
        RobotNumber.gameObject.SetActive(true);
        RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(RobotNumber.text); });
        SelectRobot("1");
        ToggleEnable(RobotOptions, true);
    }

    public void RobotUpdate(int index, RobotConfig config)
    {
        NoSections[index].GetComponentInChildren<Text>().text = config.NoSections.Value.ToString();
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



}
