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
    private bool IsEnabled = false;

    void Start()
    {
        UIE = this.gameObject.GetComponent<UIConfig>();
        UIE.panel.SetActive(false);
        StartCoroutine(LogPerformance());
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
        UIE.Overview.onClick.AddListener(delegate { ToggleOverview(); });
        SelectOption(UIE.DefaultView);
    }

    private void SelectOption(UIView selection)
    {
        //disable other objects
        UIE.View = selection;
        switch (selection)
        {
            case UIView.Robot:
                ToggleEnable();
                SetupRobotSelector();
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Performance:
                ToggleEnable();
                UIE.IsOverview = !UIE.IsOverview;
                ToggleOverview();
                CameraConfig.OverviewCamera.SetActive(true);
                CameraConfig.RobotCamera.SetActive(false);
                CameraConfig.RobotCamera.GetComponent<CameraPosition>().Clear();
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
        //toggle IsCollapsed
        if (UIE.IsCollapsed)
        {
            //UI is currently down, needs to go up
            float up = 440;
            UIE.IsCollapsed = !UIE.IsCollapsed;
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, up, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, up, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, up, 0);
            text.text = "▼"; 
            ToggleEnable();
            if (UIE.View == UIView.Robot)
            {
                SelectRobot(UIE.RobotNumber.text);
            }
            else
            {
                UIE.IsOverview = !UIE.IsOverview;
                ToggleOverview();
            }
        }
        else
        {
            //UI is currently up, needs to go down
            float down = -450;
            UIE.IsCollapsed = !UIE.IsCollapsed;
            ToggleEnable();
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, down, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, down, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, down, 0);
            text.text = "▲";
        }
    }

    //------------------------------Robot Options--------------------------------
    private void SetupRobotSelector()
    {
        ToggleEnable();
        UIE.RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(UIE.RobotNumber.text); });
        UIE.Original.onClick.AddListener(delegate { ToggleOriginal(); });
        SelectRobot(CameraConfig.CamFollow == -1 ? "1" : (CameraConfig.CamFollow + 1).ToString());
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
        if (!UIE.IsCollapsed)
        {
            //flip so that it will flip back to the default state
            UIE.IsOriginal = !UIE.IsOriginal;
            ToggleOriginal();
            //ToggleOriginal will in turn call RobotUpdate
        }
        //setup camera if not already done
        if(CameraConfig.CamFollow != Robot.RobotIndex)
        {
            Robot.AttachCam();
        }
    }

    private void RobotUpdate(RobotConfig config, bool showChanges)
    {
        //HEADER
        //Version
        Text text = UIE.VersionText;
        string original = text.text;
        text.text = config.Version.ToString();
        //Performance
        text = UIE.BestPerformanceText; //best
        text.text = Math.Round(config.Performance, 2).ToString();
        text = UIE.CurrentPerformanceText; //current
        text.text = "";
        //Body Colour
        UIE.ColourChanged.SetActive(false);
        bool isChanged = UIE.SetBodyColour(config.BodyColour.Value / 100f);
        if (showChanges && isChanged) StartCoroutine(ValueChanged(UIE.ColourChanged, config.RobotIndex));

        //BODY
        //if first time, set up body objects
        if (UIE.Bodies.Count == 0) UIE.SetupBodies(config.NoSections.Max);   
        //make sure the right number are enabled vs disabled
        for (int i = 0; i < config.NoSections.Max; i++)
        {
            UIE.Bodies[i].Body.gameObject.SetActive(i < config.NoSections.Value);
        }
        List<ObjectConfig> objConfigs = config.Configs.Where(o => o.Type == BodyPart.Body).OrderBy(o => o.Index).ToList();
        bool angleDirectionUp = true;
        for (int i = 0; i < config.NoSections.Value; i++)
        {
            BodyUI body = UIE.Bodies[i];
            BodyConfig bodyConfig = config.Configs.Where(o => o.Type == BodyPart.Body && o.Index == i).First().Body;

            //size - affects position so call first
            float value = (bodyConfig.Size.Value - bodyConfig.Size.Min) / (bodyConfig.Size.Max - bodyConfig.Size.Min);
            bool sizeChanged = UIE.SetSize(body, value);

            //position
            BodyUI prevBody = i == 0 ? null : UIE.Bodies[i - 1];
            UIE.SetBodyPosition(body, prevBody);

            //primary axis
            isChanged = UIE.SetPrimaryAxis(body, bodyConfig.RotationMultiplier.Value, bodyConfig.IsRotating.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(body.PrimaryRotation, config.RobotIndex));

            //is rotating
            isChanged = UIE.SetIsRotating(body, bodyConfig.IsRotating.Value, bodyConfig.UseSin.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(body.IsRotating, config.RobotIndex));

            //is driving
            isChanged = UIE.SetIsDriving(body, bodyConfig.IsDriving.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(body.IsDriving, config.RobotIndex));

            //drive velocity
            isChanged = UIE.SetDriveVelocity(body, bodyConfig.DriveVelocity.Value, bodyConfig.IsDriving.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(body.DriveVelocity, config.RobotIndex));

            //mass
            value = (bodyConfig.Mass.Value - bodyConfig.Mass.Min) / (bodyConfig.Mass.Max - bodyConfig.Mass.Min);
            bool massChanged = UIE.SetMass(body, value);

            if (showChanges && (sizeChanged || massChanged)) StartCoroutine(ValueChanged(body.SizeMassChanged, config.RobotIndex));

            //angle
            value = GetRelativeAngleMagnitude(bodyConfig.AngleConstraint.Value, bodyConfig.AngleConstraint.Min, bodyConfig.AngleConstraint.Max);
            isChanged = UIE.SetAngleConstraint(body, value, ref angleDirectionUp, prevBody);
            if (showChanges && i > 0 && isChanged) StartCoroutine(ValueChanged(body.JointChanged, config.RobotIndex));
        }

        //TAIL
        //if first time, set up tail object
        if (UIE.Tail == null) UIE.SetupTail();
        UIE.Tail.gameObject.SetActive(config.IsTailEnabled.Value);
        if (config.IsTailEnabled.Value)
        {
            TailUI tail = UIE.Tail;
            TailConfig tailConfig = config.Configs.Where(o => o.Type == BodyPart.Tail).First().Tail;

            //Length
            float value = (tailConfig.Length.Value - tailConfig.Length.Min) / (tailConfig.Length.Max - tailConfig.Length.Min);
            float tailLength = 0;
            isChanged = UIE.SetTailLength(value, ref tailLength);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(tail.LengthChanged, config.RobotIndex));

            //position - needed to set the new length first to adjust for it in the position
            BodyUI lastBody = UIE.Bodies[config.NoSections.Value - 1];
            UIE.SetTailPosition(lastBody, tailLength);

            //primary axis
            isChanged = UIE.SetTailPrimaryAxis(tailConfig.RotationMultiplier.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(tail.PrimaryRotation, config.RobotIndex));

            //mass multiplier
            isChanged = UIE.SetTailMass(tailConfig.TailMassMultiplier.Value);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(tail.MassMultiplier, config.RobotIndex));

            //rotation multiplier
            value = GetRelativeAngleMagnitude(tailConfig.AngleConstraint.Value, tailConfig.AngleConstraint.Min, tailConfig.AngleConstraint.Max);
            isChanged = UIE.SetTailAngleConstraint(value, ref angleDirectionUp, lastBody);
            if (showChanges && isChanged) StartCoroutine(ValueChanged(tail.JointChanged, config.RobotIndex));
        }

    }

    private void ToggleOriginal()
    {
        Text text = UIE.OriginalText;
        if (!UIE.IsOriginal)
        {
            //currently showing current, move to show original
            UIE.IsOriginal = !UIE.IsOriginal;
            text.text = "Show Current";
            UIE.Version.GetComponent<Image>().color = Color.grey;
            UIE.CurrentPerformance.GetComponent<Image>().color = Color.grey;
            UIE.BestPerformance.GetComponent<Image>().color = Color.grey;
            UIE.Bodies.ForEach(o => {
                o.PrimaryRotation.color = Color.white;
                o.IsRotating.color = Color.white;
                o.IsDriving.color = Color.white;
                o.DriveVelocity.color = Color.white;
                o.SizeMassChanged.SetActive(false);
                });
            UIE.Original.GetComponent<Image>().color = Color.white; //return to white, exception to above line
            RobotUpdate(Robot.Original, false);
        }
        else
        {
            //currently showing original, move to show current
            UIE.IsOriginal = !UIE.IsOriginal;
            text.text = "Show Original";
            UIE.Version.GetComponent<Image>().color = Color.white;
            UIE.CurrentPerformance.GetComponent<Image>().color = Color.white;
            UIE.BestPerformance.GetComponent<Image>().color = Color.white;
            RobotUpdate(Robot, false);
        }
        
    }

    //-------------------------------Performance Options-----------------------------------

    private void ToggleOverview()
    {
        if (!UIE.IsOverview)
        {
            //is currently showing individual, move to show overview
            UIE.IsOverview = !UIE.IsOverview;
            UIE.OverviewText.text = "Show Breakdown";
            UIE.Individuals.ForEach(o => o.SetActive(false));
            SetupOverviewPerformance();
        }
        else
        {
            //is currently showing overview, move to show individual
            UIE.IsOverview = !UIE.IsOverview;
            UIE.OverviewText.text = "Show Overview";
            SetupIndividualPerformance();
        }
    }

    private void SetupIndividualPerformance()
    {
        UIE.SetupIndividuals();
        float  maxPerformance = AIConfig.RobotConfigs.Max(r => r.Performance);
        maxPerformance = (float)Math.Max(10, Math.Ceiling(maxPerformance / 10) * 10);
        for (int i = 0; i < AIConfig.PopulationSize; i++)
        {
            RobotConfig robot = AIConfig.RobotConfigs.First(r => r.RobotIndex == i);
            UIE.SetIndividualHeight(robot.RobotIndex, robot.Performance, maxPerformance);
        }
    }

    private void IndividualUpdate(int robotIndex)
    {
        float maxPerformance = AIConfig.RobotConfigs.Max(r => r.Performance);
        maxPerformance = (float)Math.Max(10, Math.Ceiling(maxPerformance / 10) * 10);
        bool maxChanging = maxPerformance > UIE.MaxPerformance;
        if (maxPerformance > UIE.MaxPerformance) SetupIndividualPerformance();
        else 
        {
            RobotConfig robot = AIConfig.RobotConfigs.First(r => r.RobotIndex == robotIndex);
            UIE.SetIndividualHeight(robotIndex, robot.Performance, maxPerformance);
        }
    }

    private void SetupOverviewPerformance()
    {
        UIE.SetupOverview();
        StartCoroutine(UpdateOverview());
    }

    private IEnumerator UpdateOverview()
    {
        while(IsEnabled && UIE.View == UIView.Performance && !UIE.IsCollapsed && UIE.IsOverview)
        {
            yield return new WaitForSeconds(0.5f);
            UIE.GenerateGraph();
        }
    }

    //-----------------------------------------------All Helpers----------------------------
    private void ToggleEnable()
    {
        bool IsRobotView = UIE.View == UIView.Robot;
        UIE.RobotOptions.SetActive(IsRobotView && !UIE.IsCollapsed);
        UIE.RobotNumber.gameObject.SetActive(IsRobotView);

        UIE.PerformanceOptions.SetActive(!IsRobotView && !UIE.IsCollapsed);
    }

    private IEnumerator ValueChanged(Button button, int robotIndex)
    {
        button.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            button.GetComponent<Image>().color = Color.white;
        }
    }

    private IEnumerator ValueChanged(GameObject slider, int robotIndex)
    {
        slider.SetActive(true);
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            slider.SetActive(false);
        }
    }

    private IEnumerator ValueChanged(Text text, int robotIndex)
    {
        text.color = Color.red;
        if (text.text == "")
        {
            text.fontSize = 48;
            text.text = "-";
        }
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            text.color = Color.white;
            if (text.text == "-") text.text = "";
        }
    }

    private IEnumerator MaxChanged(float maxPerformance)
    {
        UIE.LastPerformance.color = Color.red;
        yield return new WaitForSeconds(5f);
        if(UIE.MaxPerformance == maxPerformance)
        {
            UIE.LastPerformance.color = Color.white;
        }
    }

    private float GetRelativeAngleMagnitude(Vector3 vector, float min, float max)
    {
        float magnitudeMax = new Vector3(max, max, max).magnitude;
        float magnitudeMin = new Vector3(min, min, min).magnitude;
        return (vector.magnitude - magnitudeMin) / (magnitudeMax - magnitudeMin);
    }

    private IEnumerator LogPerformance()
    {
        bool LastRobotsReady = false;
        List<RobotConfig> robots = new List<RobotConfig>();
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if(AIConfig.RobotConfigs.Count == AIConfig.PopulationSize)
            {
                //at first it will need to use the performance of the actual robots
                //then it will move to use the LastRobots instead
                if (!LastRobotsReady)
                {
                    robots.Clear();
                    bool ready = true;
                    for (int i = 0; i < AIConfig.RobotConfigs.Count; i++)
                    {
                        if (AIConfig.LastRobots[i] == null)
                        {
                            robots.Add(AIConfig.RobotConfigs.First(r => r.RobotIndex == i));
                            ready = false;
                        }
                        else
                        {
                            robots.Add(AIConfig.LastRobots[i]);
                        }
                    }
                    LastRobotsReady = ready;
                }
                else
                {
                    robots = AIConfig.LastRobots.ToList();
                }

                //calculate average
                //robots have been generated
                float average = robots.Average(r => r.Performance);
                if (UIE.MeanPerformances.Count == UIE.GraphPoints)
                {
                    UIE.MeanPerformances.RemoveAt(0);
                    UIE.MeanPerformances.Add(average);
                }
                else
                {
                    UIE.MeanPerformances.Add(average);
                }
            }
        }
    }

    //------------------------------------------Accessed by GA &&|| TrappedAlgorithm------------------------------

    internal void Enable()
    {
        IsEnabled = true;
        UIE.panel.SetActive(true);
        SetupUIOptions();
        //setup toggle
        UIE.Toggle.onClick.AddListener(delegate { ToggleUI(); });
        //flip so that it will flip back to the default state
        UIE.IsCollapsed = !UIE.IsCollapsed;
        ToggleUI();
    }

    public void UpdateRobotUI(RobotConfig config)
    {
        if (IsEnabled && UIE.View == UIView.Robot && !UIE.IsOriginal && config.RobotIndex == Robot.RobotIndex && !UIE.IsCollapsed)
        {
            Robot = config;
            RobotUpdate(config, true);
        }
    }

    public void UpdatePerformance(int robotIndex, float current, float best)
    {
        if(IsEnabled && UIE.View == UIView.Robot && !UIE.IsOriginal && robotIndex == Robot.RobotIndex && !UIE.IsCollapsed)
        {
            Text text = UIE.BestPerformanceText;
            text.text = Math.Round(best, 2).ToString();
            text = UIE.CurrentPerformanceText;
            text.text = Math.Round(current, 2).ToString();
        }
        else if(IsEnabled && UIE.View == UIView.Performance && !UIE.IsOverview && !UIE.IsCollapsed)
        {
            IndividualUpdate(robotIndex);
        }
    }


}
