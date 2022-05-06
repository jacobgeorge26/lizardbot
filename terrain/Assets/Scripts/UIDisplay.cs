using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplay : MonoBehaviour
{
    private UIConfig UIE; //UI elements
    private RobotConfig Robot;
    private bool IsEnabled = false;

    void Awake()
    {
        UIE = this.gameObject.GetComponent<UIConfig>();
        UIE.panel.SetActive(false);
    }

    private bool viewListenerSet = false;
    private void SetupUIOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < Enum.GetNames(typeof(UIView)).Length; i++)
        {
            options.Add(new Dropdown.OptionData(Enum.GetName(typeof(UIView), i)));
        }
        UIE.UIOption.ClearOptions();
        UIE.UIOption.AddOptions(options);
        if (!viewListenerSet) {
            viewListenerSet = true;
            UIE.UIOption.onValueChanged.AddListener(delegate { SelectOption(UIE.View); });
        }
        SetupRobotSelector();
        SelectOption(UIE.DefaultView);
    }

    internal void SelectOption(UIView selection)
    {
        //disable other objects
        UIE.View = selection;
        switch (selection)
        {
            case UIView.Robot:
                ToggleEnable();
                UIE.IsCollapsed = !UIE.IsCollapsed;
                ToggleUI();
                SelectRobot(CameraConfig.CamFollow == -1 ? "1" : (CameraConfig.CamFollow + 1).ToString());
                CameraConfig.RobotCamera.SetActive(true);
                CameraConfig.OverviewCamera.SetActive(false);
                CameraConfig.Hat.SetActive(true);
                break;
            case UIView.Overview:
                if (!UIE.IsCollapsed)
                {
                    ToggleUI(true);
                }
                ToggleEnable();
                CameraConfig.OverviewCamera.SetActive(true);
                CameraConfig.RobotCamera.SetActive(false);
                CameraConfig.RobotCamera.GetComponent<CameraPosition>().Clear();
                CameraConfig.Hat.SetActive(false);
                CameraConfig.Hat.transform.parent = CameraConfig.RobotCamera.transform;
                break;
            default:
                break;
        }
    }

    //--------------------------------------------Toggle-------------------------------------
    private void ToggleUI(bool skipToggle = false)
    {
        Text text = UIE.ToggleText;
        //toggle IsCollapsed
        if (UIE.IsCollapsed)
        {
            //UI is currently down, needs to go up
            float up = 440;
            if (!skipToggle) UIE.IsCollapsed = !UIE.IsCollapsed;
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
        }
        else
        {
            //UI is currently up, needs to go down
            float down = -450;
            if(!skipToggle) UIE.IsCollapsed = !UIE.IsCollapsed;
            ToggleEnable();
            UIE.panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            UIE.UIOption.transform.localPosition = new Vector3(-800, down, 0);
            UIE.RobotNumber.transform.localPosition = new Vector3(-525, down, 0);
            UIE.Toggle.transform.localPosition = new Vector3(920, down, 0);
            text.text = "▲";
        }
    }

    //------------------------------Robot Options--------------------------------
    private bool numberListenerSet = false, originalListenerSet = false;
    private void SetupRobotSelector()
    {
        if (!numberListenerSet) {
            numberListenerSet = true;
            UIE.RobotNumber.onEndEdit.AddListener(delegate { SelectRobot(UIE.RobotNumber.text); });
        }
        if (!originalListenerSet)
        {
            originalListenerSet = true;
            UIE.Original.onClick.AddListener(delegate { ToggleOriginal(); });
        }
    }

    internal void SelectRobot(string robotText)
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
        //let robot cam know where to look
        //needs repeating in case this is after a respawn - robot index would still be the same
        Robot.AttachCam(); 
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
            BodyConfig bodyConfig = null;
            try { bodyConfig = config.Configs.First(o => o.Type == BodyPart.Body && o.Index == i).Body; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), config); return; }

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
            bool massChanged = UIE.SetBodyMass(body, value);

            if (showChanges && (sizeChanged || massChanged)) StartCoroutine(ValueChanged(body.SizeMassChanged, config.RobotIndex));

            //angle
            value = GetRelativeAngleMagnitude(bodyConfig.AngleConstraint.Value, bodyConfig.AngleConstraint.Min, bodyConfig.AngleConstraint.Max);
            isChanged = UIE.SetAngleConstraint(body, value, ref angleDirectionUp, prevBody);
            if (showChanges && i > 0 && isChanged) StartCoroutine(ValueChanged(body.JointChanged, config.RobotIndex));
        }

        //LEGS
        //if first time, set up legs
        if (UIE.Legs.Count == 0) UIE.SetupLegs(config.NoSections.Max * 2);
        List<ObjectConfig> legConfigs = config.Configs.Where(o => o.Type == BodyPart.Leg).OrderBy(o => o.Index).ToList();
        for (int i = 0; i < config.NoSections.Max * 2; i++)
        {
            List<ObjectConfig> legs = legConfigs.Where(l => l.Leg.AttachedBody == Mathf.FloorToInt(i / 2) && (int)l.Leg.Position == i % 2).ToList();
            LegUI leg = UIE.Legs[i];
            //make sure the right number are enabled vs disabled
            leg.Leg.gameObject.SetActive(legs.Count > 0);
            if(legs.Count > 0)
            {
                //get leg
                LegConfig legConfig = null;
                try { legConfig = legs.First().Leg; }
                catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), config); return; }
                //get prevBody
                ObjectConfig attachedBody = null;
                try { attachedBody = config.Configs.First(o => o.Type == BodyPart.Body && o.Index == legConfig.AttachedBody); }
                catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), config); return; }

                //size - affects position so call first
                float value = (legConfig.Length.Value - legConfig.Length.Min) / (legConfig.Length.Max - legConfig.Length.Min);
                float legLength = 0;
                bool lengthChanged = UIE.SetLegLength(leg, value, ref legLength);

                //position
                BodyUI prevBody = UIE.Bodies[attachedBody.Index];
                UIE.SetLegPosition(leg, prevBody, legLength, (legConfig.Position == LegPosition.Left ? -1 : 1));

                //mass
                value = (legConfig.Mass.Value - legConfig.Mass.Min) / (legConfig.Mass.Max - legConfig.Mass.Min);
                bool massChanged = UIE.SetLegMass(leg, value);

                //offset
                isChanged = UIE.SetLegOffset(leg, legConfig.AngleOffset.Value);
                if (showChanges && isChanged) StartCoroutine(ValueChanged(leg.Offset, config.RobotIndex));

                //gait multiplier
                isChanged = UIE.SetGaitMultiplier(leg, legConfig.GaitMultiplier.Value);
                if (showChanges && isChanged) StartCoroutine(ValueChanged(leg.GaitMultiplier, config.RobotIndex));

                if (showChanges && (lengthChanged || massChanged)) StartCoroutine(ValueChanged(leg.MassLengthChanged, config.RobotIndex));
            }
        }

        //TAIL
        //if first time, set up tail object
        if (UIE.Tail == null) UIE.SetupTail();
        UIE.Tail.gameObject.SetActive(config.IsTailEnabled.Value);
        if (config.IsTailEnabled.Value)
        {
            TailUI tail = UIE.Tail;
            TailConfig tailConfig = null;
            try { tailConfig = config.Configs.First(o => o.Type == BodyPart.Tail).Tail; }
            catch (Exception ex) { GameController.Controller.SingleRespawn(ex.ToString(), config); return; }

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

    //-----------------------------------------------All Helpers----------------------------
    private void ToggleEnable()
    {
        bool IsRobotView = UIE.View == UIView.Robot;
        UIE.RobotOptions.SetActive(IsRobotView && !UIE.IsCollapsed);
        UIE.RobotNumber.gameObject.SetActive(IsRobotView);
        UIE.Toggle.gameObject.SetActive(IsRobotView);
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

    private float GetRelativeAngleMagnitude(Vector3 vector, float min, float max)
    {
        float magnitudeMax = new Vector3(max, max, max).magnitude;
        float magnitudeMin = new Vector3(min, min, min).magnitude;
        return (vector.magnitude - magnitudeMin) / (magnitudeMax - magnitudeMin);
    }

    //------------------------------------------Accessed by GA &&|| TrappedAlgorithm------------------------------
    private bool toggleListenerSet = false;
    internal void Enable()
    {
        IsEnabled = true;
        UIConfig.UIContainer.SetActive(true);
        UIE.panel.SetActive(true);
        SetupUIOptions();
        //setup toggle
        if (!toggleListenerSet)
        {
            toggleListenerSet = true;
            UIE.Toggle.onClick.AddListener(delegate { ToggleUI(); });
        }
        //flip so that it will flip back to the default state
        UIE.IsCollapsed = !UIE.IsCollapsed;
        ToggleUI();
    }

    internal void Disable()
    {
        IsEnabled = false;
        UIConfig.UIContainer.SetActive(false);
        UIE.panel.SetActive(false);
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
    }

    public int GetCurrentRobot()
    {
        return Robot == null ? -1 : Robot.RobotIndex;
    }


}
