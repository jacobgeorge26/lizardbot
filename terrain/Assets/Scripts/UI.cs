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

        //body diagram
        //too many bodies, clear out some
        while(UIE.Bodies.Count - config.NoSections.Value > 0)
        {
            UITemplateBody removeBody = UIE.Bodies[UIE.Bodies.Count - 1];
            Destroy(removeBody.Body.gameObject);
            UIE.Bodies.Remove(removeBody);
        }
        //not enough bodies, create some
        while(config.NoSections.Value - UIE.Bodies.Count > 0)
        {
            GameObject newBody = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BodyUI"));
            UITemplateBody objects = newBody.GetComponent<UITemplateBody>();
            UIE.Bodies.Add(objects);
            newBody.transform.SetParent(UIE.RobotNumber.transform.parent);
            newBody.name = $"Body {UIE.Bodies.IndexOf(objects)}";
        }
        List<ObjectConfig> objConfigs = config.Configs.Where(o => o.Type == BodyPart.Body).OrderBy(o => o.Index).ToList();
        for (int i = 0; i < UIE.Bodies.Count; i++)
        {
            UITemplateBody body = UIE.Bodies[i];
            BodyConfig bodyConfig = config.Configs.Where(o => o.Type == BodyPart.Body && o.Index == i).First().Body;

            int width = 160;
            //position
            UITemplateBody prevBody = i == 0 ? null : UIE.Bodies[i - 1];
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

            //angle - 0 - 60
            //size
            float originalScale = body.Body.transform.localScale.x;
            float scalemin = 0.7f, scalemax = 1f;
            float value = (bodyConfig.Size.Value - bodyConfig.Size.Min) / (bodyConfig.Size.Max - bodyConfig.Size.Min);
            float newScale = value * (scalemax - scalemin) + scalemin;
            body.Body.transform.localScale = new Vector3(newScale, newScale, newScale);

            //mass
            float originalMass = body.Body.GetComponent<Image>().pixelsPerUnitMultiplier;
            float massmin = 0.3f, massmax = 1f;
            value = (bodyConfig.Mass.Value - bodyConfig.Mass.Min) / (bodyConfig.Mass.Max - bodyConfig.Mass.Min);
            float newMass = value * (massmax - massmin) + massmin;
            body.Body.GetComponent<Image>().pixelsPerUnitMultiplier = newMass;

            //angle
            float originalAngle = body.Body.transform.localPosition.y;
            float anglemin = 0, anglemax = 60;
            value = bodyConfig.AngleConstraint.Value.magnitude;

            if(ChangeColour && (originalScale != newScale || originalMass != newMass)) StartCoroutine(TextChanged(body.Changed, config.RobotIndex));

        }

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

    private IEnumerator TextChanged(GameObject button, int robotIndex)
    {
        button.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(5f);
        if (Robot.RobotIndex == robotIndex)
        {
            button.GetComponent<Image>().color = Color.white;
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

    //------------------------------------------Accessed by GA------------------------------

    public void UpdateRobotUI(RobotConfig config)
    {
        if (UIE.View == UIView.Robot && !UIE.IsOriginal && config.RobotIndex == Robot.RobotIndex) RobotUpdate(config, true);
    }

}
