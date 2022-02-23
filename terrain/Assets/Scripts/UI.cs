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
                ToggleEnable();
                SetupRobotSelector();
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Performance:
                ToggleEnable();
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
            UIE.IsCollapsed = !UIE.IsCollapsed;
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, 440, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, 176, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, 440, 0);
            text.text = "▼"; 
            ToggleEnable();
            if (UIE.View == UIView.Robot)
            {
                SelectRobot(UIE.RobotNumber.text);
            }
        }
        else
        {
            //UI is currently up, needs to go down
            UIE.IsCollapsed = !UIE.IsCollapsed;
            ToggleEnable();
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, -450, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, -180, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, -450, 0);
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
        ToggleEnable();
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

    private void RobotUpdate(RobotConfig config, bool ChangeColour)
    {
        //↶ ↷ ← symbols that will be used
        Text text;
        string original;

        //Version
        text = UIE.VersionText;
        original = text.text;
        text.text = config.Version.ToString();
        if (ChangeColour && text.text != original) StartCoroutine(TextChanged(UIE.Version, config.RobotIndex));

        //Performance
        //best
        text = UIE.BestPerformanceText;
        text.text = config.Performance.ToString();
        //current
        text = UIE.CurrentPerformanceText;
        text.text = "";

        //body diagram
        //if first time, set up body objects
        if (UIE.Bodies.Count == 0)
        {
            for (int i = 0; i < config.NoSections.Max; i++)
            {
                GameObject newBody = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BodyUI"));
                BodyUI objects = newBody.GetComponent<BodyUI>();
                UIE.Bodies.Add(objects);
                newBody.transform.SetParent(UIE.RobotNumber.transform.parent);
                newBody.name = $"Body {UIE.Bodies.IndexOf(objects)}";
            }
        }
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

            int width = 160;
            //position
            BodyUI prevBody = i == 0 ? null : UIE.Bodies[i - 1];
            body.Body.transform.localPosition = new Vector3(prevBody == null ? -880 : prevBody.Body.transform.localPosition.x + prevBody.Body.transform.localScale.x * width + 20, 30, 0);
            body.Body.transform.localScale = new Vector3(1, 1, 1);

            //primary axis
            text = body.PrimaryRotation;
            original = text.text;
            text.text = GetPrimaryAxis(bodyConfig.RotationMultiplier.Value);
            if (ChangeColour && text.text != original) StartCoroutine(TextChanged(body.PrimaryRotation, config.RobotIndex));

            //is rotating
            text = body.IsRotating;
            original = text.text;
            text.text = !bodyConfig.IsRotating.Value ? ""
                : bodyConfig.UseSin.Value ? "↶" : "↷";
            if (ChangeColour && text.text != original) StartCoroutine(TextChanged(body.IsRotating, config.RobotIndex));

            //is driving
            text = body.IsDriving;
            original = text.text;
            text.text = bodyConfig.IsDriving.Value ? "←" : "";
            if (ChangeColour && text.text != original) StartCoroutine(TextChanged(body.IsDriving, config.RobotIndex));

            //drive velocity
            text = body.DriveVelocity;
            original = text.text;
            text.text = Math.Round(bodyConfig.DriveVelocity.Value, 1).ToString();
            if (ChangeColour && text.text != original) StartCoroutine(TextChanged(body.DriveVelocity, config.RobotIndex));

            //size
            float originalScale = body.RelativeScale;
            float scalemin = 0.7f, scalemax = 1f;
            float value = (bodyConfig.Size.Value - bodyConfig.Size.Min) / (bodyConfig.Size.Max - bodyConfig.Size.Min);
            float newScale = value * (scalemax - scalemin) + scalemin;
            body.RelativeScale = newScale;
            body.Body.transform.localScale = new Vector3(newScale, newScale, newScale);

            //mass
            float originalMass = body.Body.GetComponent<Image>().pixelsPerUnitMultiplier;
            float massmin = 0.3f, massmax = 1f;
            value = (bodyConfig.Mass.Value - bodyConfig.Mass.Min) / (bodyConfig.Mass.Max - bodyConfig.Mass.Min);
            float newMass = value * (massmax - massmin) + massmin;
            body.Body.GetComponent<Image>().pixelsPerUnitMultiplier = newMass;

            //angle
            float originalAngle = body.RelativeAngle;
            float anglemin = 0, anglemax = 60;
            value = GetRelativeAngleMagnitude(bodyConfig.AngleConstraint.Value, bodyConfig.AngleConstraint.Min, bodyConfig.AngleConstraint.Max);
            //angle needs to be relative to previous body UI
            float prevAngle = i == 0 ? 0 : prevBody.Body.transform.localPosition.y;
            //new angle is how far the new body should be from the previous body, put into the scale anglemin - anglemax
            float newAngle = i == 0 ? (anglemax + anglemin) / 2 : (value * (anglemax - anglemin) + anglemin) / 2;
            body.RelativeAngle = newAngle;

            float actualAngle;
            if(prevAngle + newAngle > anglemax && prevAngle - newAngle < anglemin)
            {
                //handle the case in which the angle exceeds both min and max
                //data shown won't quite be accurate but will be close enough
                actualAngle = anglemax - prevAngle > prevAngle - anglemin ? anglemax : anglemin;
                angleDirectionUp = anglemax - prevAngle > prevAngle - anglemin ? false : true;
            }
            else
            {
                actualAngle = angleDirectionUp ? prevAngle + newAngle : prevAngle - newAngle;
                //if new angle exceeds max, then correct and move back down
                angleDirectionUp = actualAngle > anglemax ? false : angleDirectionUp;
                actualAngle = actualAngle > anglemax ? prevAngle - newAngle : actualAngle;
                //if new angle is below min, then correct and move back up
                angleDirectionUp = actualAngle < anglemin ? true : angleDirectionUp;
                actualAngle = actualAngle < anglemin ? prevAngle + newAngle : actualAngle;
            }
            body.Body.transform.localPosition = new Vector3(body.Body.transform.localPosition.x, actualAngle, 0);

            //if(originalAngle != newAngle)

            if (ChangeColour && (originalScale != newScale || originalMass != newMass)) StartCoroutine(TextChanged(body.SizeMassChanged, config.RobotIndex));

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

    //-----------------------------------------------All Helpers----------------------------
    private void ToggleEnable()
    {
        bool IsRobotView = UIE.View == UIView.Robot;
        UIE.RobotNumber.transform.parent.gameObject.SetActive(IsRobotView && !UIE.IsCollapsed);
        UIE.RobotNumber.gameObject.SetActive(IsRobotView);

        UIE.Overview.transform.parent.gameObject.SetActive(!IsRobotView && !UIE.IsCollapsed);
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

    private IEnumerator TextChanged(GameObject slider, int robotIndex)
    {
        slider.SetActive(true);
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            slider.SetActive(false);
        }
    }

    private IEnumerator TextChanged(Text text, int robotIndex)
    {
        text.color = Color.red;
        yield return new WaitForSeconds(5f);
        if(Robot.RobotIndex == robotIndex)
        {
            text.color = Color.white;
        }
    }

    private string GetPrimaryAxis(Vector3 vector)
    {
        if (vector.x > vector.y && vector.x > vector.y) return "X";
        else if (vector.y > vector.z) return "Y";
        else return "Z";
    }

    private float GetRelativeAngleMagnitude(Vector3 vector, float min, float max)
    {
        float magnitudeMax = new Vector3(max, max, max).magnitude;
        float magnitudeMin = new Vector3(min, min, min).magnitude;
        return (vector.magnitude - magnitudeMin) / (magnitudeMax - magnitudeMin);
    }

    //------------------------------------------Accessed by GA &&|| TrappedAlgorithm------------------------------

    public void UpdateRobotUI(RobotConfig config)
    {
        if (UIE.View == UIView.Robot && !UIE.IsOriginal && config.RobotIndex == Robot.RobotIndex)
        {
            Robot = config;
            RobotUpdate(config, true);
        }
    }

    public void UpdatePerformance(int robotIndex, float current, float best)
    {
        if(UIE.View == UIView.Robot && !UIE.IsOriginal && robotIndex == Robot.RobotIndex)
        {
            Text text = UIE.BestPerformanceText;
            text.text = best.ToString();
            text = UIE.CurrentPerformanceText;
            text.text = current.ToString();
        }
    }

}
