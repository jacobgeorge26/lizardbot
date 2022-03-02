using Config;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConfig : MonoBehaviour
{
    //Important - how static scripts will access the UI (aka GA to update robot)
    public static GameObject UIContainer;

    [Header("Default Options")]
    //UI options
    public static bool IsUIEnabled = true;
    [HideInInspector]
    public UIView DefaultView = UIView.Performance;
    [HideInInspector]
    public bool IsCollapsed = true;
    [HideInInspector]
    public bool IsOriginal = false;
    [HideInInspector]
    public bool IsOverview = true;
    //two samples taken per second
    public static int NoPoints = 240;
    public static int SampleSize = 10;

    [Header("General Objects")]
    public GameObject panel;
    public Dropdown UIOption;
    public Button Toggle;
    public GameObject RobotOptions;
    public GameObject PerformanceOptions;
    //Helpers for both
    [HideInInspector]
    public Text ToggleText;
    [HideInInspector]
    public UIView View
    {
        get => (UIView)UIOption.value;
        set => UIOption.value = (int)value;
    }

    [Header("Robot Objects")]
    public InputField RobotNumber;
    public Button Original;
    public Button Version;
    public Button CurrentPerformance;
    public Button BestPerformance;
    public Image BodyColour;
    public GameObject ColourChanged;

    //Text objects for robot display - save repeated calls for GetComponent
    [HideInInspector]
    public Text OriginalText;
    [HideInInspector]
    public Text VersionText;
    [HideInInspector]
    public Text CurrentPerformanceText;
    [HideInInspector]
    public Text BestPerformanceText;

    //used for robot UI
    [HideInInspector]
    public List<BodyUI> Bodies;
    [HideInInspector]
    public TailUI Tail;

    [Header("Performance Objects")]
    public Button Overview;
    public Text FirstX;
    public Text LastX;
    public Text FirstPerformance;
    public Text LastPerformance;

    //Text objects for performance display
    [HideInInspector]
    public Text OverviewText;
    [HideInInspector]
    public List<float> MeanPerformances = new List<float>();
    [HideInInspector]
    public List<GameObject> Individuals = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Overviews = new List<GameObject>();
    [HideInInspector]
    public float MaxPerformance = 0;


    void Awake()
    {
        UIContainer = this.gameObject;

        //setup text objects
        ToggleText = Toggle.GetComponentInChildren<Text>();
        OriginalText = Original.GetComponentInChildren<Text>();
        VersionText = Version.GetComponentInChildren<Text>();
        CurrentPerformanceText = CurrentPerformance.GetComponentInChildren<Text>();
        BestPerformanceText = BestPerformance.GetComponentInChildren<Text>();
        OverviewText = Overview.GetComponentInChildren<Text>();
    }

    //-------------------------------------------------------------------------------------------
    //helper methods to make UI a bit easier to read
    private float anglemin = 0, anglemax = 60;
    private float scalemin = 0.7f, scalemax = 0.9f;
    private float massmin = 0.2f, massmax = 0.7f;
    private float lengthmin = 160, lengthmax = 250;
    private float performanceWidth = 1755, performanceHeight = 250;
    private float performanceMinX = -835, performanceMinY = -140;

    public void SetupBodies(int max)
    {
        for (int i = 0; i < max; i++)
        {
            GameObject newBody = MonoBehaviour.Instantiate(Resources.Load<GameObject>("BodyUI"));
            BodyUI objects = newBody.GetComponent<BodyUI>();
            Bodies.Add(objects);
            newBody.transform.SetParent(RobotOptions.transform);
            newBody.name = $"Body {i}";
        }
    }

    public bool SetBodyColour(float colour)
    {
        BodyColour.transform.localPosition = new Vector3(-910, (anglemax + anglemin) / 2, 0);
        Color oldColour = BodyColour.color;
        Color newColour = new Color(colour, colour, 1f);
        BodyColour.color = newColour;
        return oldColour.r != newColour.r;
    }

    public void SetBodyPosition(BodyUI body, BodyUI prevBody)
    {
        float x = GetXPos(body.Body.transform, body.RelativeScale, prevBody == null ? null : prevBody.Body.transform, prevBody == null ? 0 : prevBody.RelativeScale);
        body.Body.transform.localPosition = new Vector3(x, (anglemax + anglemin) / 2, 0);
    }

    private float GetXPos(Transform thisBody, float thisScale, Transform prevBody, float prevScale)
    {
        float prevWidth = prevBody == null ? 0 : prevBody.GetComponent<RectTransform>().rect.width;
        float thisWidth = thisBody.GetComponent<RectTransform>().rect.width;
        float x = prevBody == null ? -780 : prevBody.localPosition.x; //start point to work right from
        x += prevBody == null ? 0 : ((prevScale * prevWidth) / 2) + ((thisScale * thisWidth) / 2) + 10; //get x position using scale & width, from previous object
        return x;
    }

    public bool SetPrimaryAxis(BodyUI body, Vector3 value, bool IsRotating)
    {
        string original = body.PrimaryRotation.text;
        body.PrimaryRotation.fontSize = 34;
        body.PrimaryRotation.text = IsRotating ? GetPrimaryAxis(value) : "";
        return body.PrimaryRotation.text != original;
    }

    private string GetPrimaryAxis(Vector3 vector)
    {
        if (vector.x > vector.y && vector.x > vector.y) return "X";
        else if (vector.y > vector.z) return "Y";
        else return "Z";
    }

    public bool SetIsRotating(BodyUI body, bool isRotating, bool isSin)
    {
        string original = body.IsRotating.text;
        body.IsRotating.text = !isRotating ? ""
            : isSin ? "↶" : "↷";
        return body.IsRotating.text != original;
    }

    public bool SetIsDriving(BodyUI body, bool isDriving)
    {
        string original = body.IsDriving.text;
        body.IsDriving.text = isDriving ? "←" : "";
        return body.IsDriving.text != original;
    }

    public bool SetDriveVelocity(BodyUI body, float driveVelocity, bool isDriving)
    {
        string original = body.DriveVelocity.text;
        body.DriveVelocity.fontSize = 34;
        body.DriveVelocity.text = isDriving ? Math.Round(driveVelocity, 2).ToString() : "";
        return body.DriveVelocity.text != original;
    }

    public bool SetSize(BodyUI body, float value)
    {
        float originalScale = body.RelativeScale;
        float newScale = value * (scalemax - scalemin) + scalemin;
        body.RelativeScale = newScale;
        body.Body.transform.localScale = new Vector3(newScale, newScale, newScale);
        return newScale != originalScale;
    }

    public bool SetMass(BodyUI body, float value)
    {
        float originalMass = body.Body.GetComponent<Image>().pixelsPerUnitMultiplier;
        float newMass = value * (massmax - massmin) + massmin;
        body.Body.GetComponent<Image>().pixelsPerUnitMultiplier = newMass;
        return newMass != originalMass;
    }

    public bool SetAngleConstraint(BodyUI body, float value, ref bool angleDirectionUp, BodyUI prevBody)
    {
        float originalAngle = body.RelativeAngle;
        //angle needs to be relative to previous body UI
        float prevAngle = prevBody == null ? 0 : prevBody.Body.transform.localPosition.y;
        //new angle is how far the new body should be from the previous body, put into the scale anglemin - anglemax
        float newAngle = prevBody == null ? (anglemax + anglemin) / 2 : (value * (anglemax - anglemin) + anglemin) / 2;
        body.RelativeAngle = newAngle;

        //set actual angle
        float actualAngle = GetActualAngle(prevAngle, newAngle, ref angleDirectionUp);
        body.Body.transform.localPosition = new Vector3(body.Body.transform.localPosition.x, actualAngle, 0);
        return newAngle != originalAngle;
    }

    private float GetActualAngle(float prevAngle, float newAngle, ref bool angleDirectionUp)
    {
        float actualAngle;
        if (prevAngle + newAngle > anglemax && prevAngle - newAngle < anglemin)
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
        return actualAngle;
    }

    public void SetupTail()
    {
        GameObject newTail = MonoBehaviour.Instantiate(Resources.Load<GameObject>("TailUI"));
        TailUI objects = newTail.GetComponent<TailUI>();
        Tail = objects;
        newTail.transform.SetParent(RobotOptions.transform);
        newTail.name = $"Tail";
    }

    public bool SetTailLength(float value, ref float newLength)
    {
        RectTransform lengthObject = Tail.transform.GetComponent<RectTransform>();
        float originalLength = lengthObject.rect.width;
        newLength = value * (lengthmax - lengthmin) + lengthmin;
        lengthObject.sizeDelta = new Vector2(newLength, lengthObject.rect.height);
        return newLength != originalLength;
    }

    public void SetTailPosition(BodyUI lastBody, float tailLength)
    {
        float x = GetXPos(Tail.transform, Tail.RelativeScale, lastBody.Body.transform, lastBody.RelativeScale);
        Tail.transform.localPosition = new Vector3(x, (anglemax + anglemin) / 2, 0);
        Tail.transform.localScale = new Vector3(1, 1, 1);
        //set the position of the JointChanged slider - only other position affected by the tail length
        Tail.JointChanged.transform.localPosition = new Vector3((-tailLength / 2) - 5, 0, 0);
    }

    public bool SetTailPrimaryAxis(Vector3 rotation)
    {
        string original = Tail.PrimaryRotation.text;
        Tail.PrimaryRotation.text = GetPrimaryAxis(rotation);
        return Tail.PrimaryRotation.text != original;
    }

    public bool SetTailMass(float value)
    {
        string original = Tail.MassMultiplier.text;
        Tail.MassMultiplier.text = Math.Round(value, 2).ToString();
        return Tail.MassMultiplier.text != original;
    }

    public bool SetTailAngleConstraint(float value, ref bool angleDirectionUp, BodyUI lastBody)
    {
        float originalAngle = Tail.RelativeAngle;
        //angle needs to be relative to the last body UI
        float prevAngle = lastBody.Body.transform.localPosition.y;
        //new angle is how far the new tail should be from the previous tail, put into the scale anglemin - anglemax
        float newAngle = (value * (anglemax - anglemin) + anglemin) / 2;
        Tail.RelativeAngle = newAngle;
        //set actual angle
        float actualAngle = GetActualAngle(prevAngle, newAngle, ref angleDirectionUp);
        Tail.transform.localPosition = new Vector3(Tail.transform.localPosition.x, actualAngle, 0);
        return newAngle != originalAngle;
    }

    public void SetupIndividuals()
    {
        //if individuals is empty then this is the first time this is being called
        //create all the objects
        if(Individuals.Count == 0)
        {
            float width = (performanceWidth / AIConfig.PopulationSize) - 5;
            float initY = 10;
            for (int i = 0; i < AIConfig.PopulationSize; i++)
            {
                GameObject graph = MonoBehaviour.Instantiate(Resources.Load<GameObject>("IndividualPerformanceUI"));
                Individuals.Add(graph);
                //update parent
                graph.transform.SetParent(PerformanceOptions.transform);
                //update size
                RectTransform transform = graph.GetComponent<RectTransform>();
                transform.localScale = new Vector3(1, 1, 1);
                transform.sizeDelta = new Vector2(width, initY);
                //update position
                float thisX = performanceMinX + ((width + 5) * i) + (width / 2) + 5;
                graph.transform.localPosition = new Vector3(thisX, performanceMinY + (initY / 2), 0);
                graph.name = $"Robot {i + 1}";
            }
        }
        FirstX.text = $"Robot 1";
        LastX.text = $"Robot {AIConfig.PopulationSize}";
        LastPerformance.text = MaxPerformance.ToString();
    }

    public void SetIndividualHeight(int index, float performance, float maxPerformance)
    {
        MaxPerformance = Math.Max(MaxPerformance, maxPerformance);
        LastPerformance.text = MaxPerformance.ToString();
        GameObject graph = Individuals[index];
        graph.SetActive(true);
        RectTransform transform = graph.GetComponent<RectTransform>();
        float value = performance / MaxPerformance;
        float newHeight = value * performanceHeight;
        transform.sizeDelta = new Vector2(transform.rect.width, newHeight);
        float thisY = performanceMinY + (newHeight / 2);
        graph.transform.localPosition = new Vector3(graph.transform.localPosition.x, thisY, 0);
    }

    public void SetupOverview()
    {
        if(Overviews.Count == 0)
        {
            for (int i = 0; i < NoPoints / SampleSize; i++)
            {
                GameObject graph = MonoBehaviour.Instantiate(Resources.Load<GameObject>("GraphLineUI"));
                Overviews.Add(graph);
                //update parent
                graph.transform.SetParent(PerformanceOptions.transform);
                graph.name = $"Line {i}";
                RectTransform transform = graph.GetComponent<RectTransform>();
                transform.localScale = new Vector3(1, 1, 1);
                transform.sizeDelta = new Vector2(10, 10);
            }
        }
        FirstX.text = $"-{NoPoints / 2}s";
        LastX.text = "0s";


    }

    public void GenerateGraph(float maxPerformance)
    {
        MaxPerformance = Math.Max(MaxPerformance, maxPerformance);
        LastPerformance.text = MaxPerformance.ToString();
        float width = (performanceWidth / (NoPoints / SampleSize));
        int startIndex = MeanPerformances.Count - 1 - (MeanPerformances.Count % SampleSize);
        for (int i = startIndex; i > SampleSize; i -= SampleSize)
        {
            int index = i / SampleSize;
            GameObject line = Overviews[index];
            line.SetActive(true);
            float point1 = MeanPerformances[i - SampleSize], point2 = MeanPerformances[i];
            float x2 = (performanceMinX + performanceWidth) - ((width * (startIndex - i) / SampleSize) + (width / 2)), x1 = x2 - width;
            float y1 = performanceMinY + ((point1 / MaxPerformance) * performanceHeight);
            float y2 = performanceMinY + ((point2 / MaxPerformance) * performanceHeight);
            float m = (y2 - y1) / (x2 - x1);

            float distance = Vector3.Distance(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
            double radians = Math.Atan(m);
            double angle = radians * (180 / Math.PI);

            line.transform.localPosition = new Vector3((x1 + x2) / 2, (y1 + y2) / 2, 0);
            line.transform.rotation = Quaternion.Euler(0, 0, (float)angle);
            line.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 10);
        }
    }
}


