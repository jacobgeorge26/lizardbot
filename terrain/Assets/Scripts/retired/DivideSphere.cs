//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class DivideSphere : MonoBehaviour
//{

//Dumping visualisation for dynamic movement here
////visualise whole segment
//foreach (Vector3 item in filteredVectors)
//{
//    GameObject spherePoint = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
//    spherePoint.transform.localScale = new Vector3(1f, 1f, 1f);
//    spherePoint.transform.position = item + newPosition;
//}

//private void Visualise(Vector3 spawnPoint, List<Vector3> filteredVectors, Vector3 newPosition)
//{
//    //origin
//    GameObject origin = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
//    origin.transform.localScale = new Vector3(1f, 1f, 1f);
//    origin.transform.position = spawnPoint;
//    //if there are viable options available then move toward the best option
//    foreach (var point in filteredVectors)
//    {
//        //trajectory
//        GameObject direction = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
//        direction.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
//        direction.transform.position = point + newPosition;
//    }
//}


//    private GameObject parent;
//    // Start is called before the first frame update
//    void Start()
//    {
//        int samples = 50, radius = 10;
//        parent = new GameObject();

//        Vector3[] points = GetSpherePoints(samples, radius);
//        GameObject sphere = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Sphere"));
//        sphere.transform.parent = parent.transform;
//        sphere.transform.localScale = new Vector3(radius  * 2, radius * 2, radius * 2);
//        sphere.transform.localPosition = Vector3.zero;

//        GameObject centre = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
//        centre.transform.parent = parent.transform;
//        centre.transform.localScale = new Vector3(radius / 10, radius / 10, radius / 10);
//        centre.transform.localPosition = Vector3.zero;

//        parent.transform.position = new Vector3(14, 10, 10);

//        GameObject original = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Direction"));
//        original.transform.parent = parent.transform;
//        original.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
//        original.transform.localPosition = points[0];

//        Vector3 rotation = new Vector3(0, 0, 90);
//        Vector3 spawnPoint = Vector3.zero;
//        Vector3 direction = centre.transform.position - spawnPoint;

//        for (int i = 0; i < samples; i++)
//        {      
//            points[i] = RotatePointAroundPivot(points[i], centre.transform.localPosition, rotation);
//        }


//        Vector3 pointVector = points[0];
//        GameObject point = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Direction"));
//        point.transform.parent = parent.transform;
//        point.transform.localScale = new Vector3(1f, 1f, 1f);
//        point.transform.localPosition = pointVector;

//        pointVector = points.OrderBy(p => Vector3.Angle(p, direction)).First();
//        GameObject turn = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Point"));
//        turn.transform.parent = parent.transform;
//        turn.transform.localScale = new Vector3(1f, 1f, 1f);
//        turn.transform.localPosition = pointVector;
//    }

//    //This method was taken from this unity forum post
//    //https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html
//    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
//    {
//        return Quaternion.Euler(angles) * (point - pivot) + pivot;
//    }

//    //This method was taken from this stackoverflow post
//    //https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere
//    private Vector3[] GetSpherePoints(int samples, int radius)
//    {
//        Vector3[] points = new Vector3[samples];
//        var phi = Mathf.PI * (3 - Mathf.Sqrt(5));
//        for (int i = 0; i < samples; i++)
//        {
//            /////////////////
//            GameObject point = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Stuck"));
//            point.transform.parent = parent.transform;
//            point.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

//            var y = 1 - (i / (samples - 1f)) * 2;
//            var r = Mathf.Sqrt(1 - y * y);
//            var theta = phi * i;
//            var x = Mathf.Cos(theta) * r;
//            var z = Mathf.Sin(theta) * r;

//            /////////////////////
//            point.transform.localPosition = new Vector3(x, y, z) * radius;

//            points[i] = new Vector3(x, y, z) * radius;
//        }
//        return points;
//    }
//}
