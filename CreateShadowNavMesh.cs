using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CreateShadowNavMesh : MonoBehaviour
{
    // Start is called before the first frame update
    RaycastHit hit;
    public GameObject[] objs;
    public List<GameObject> shadows = new List<GameObject>();

    public GameObject NavMeshBaker;
    private List<GameObject> shadowNavMeshes = new List<GameObject>();

    public Material material;
    private int agentTypeID;

    Vector3[] vertices;
    Vector3[] worldSpacVertices;
    Vector3[] filteredVertices;

    private List<GameObject> hitIndicators = new List<GameObject>();
    private List<GameObject> convexIndicators = new List<GameObject>();

    private void Awake()
    {
        foreach (GameObject mesh in objs)
        {
            //create a shadow nav mesh perobject
            GameObject go = new GameObject();
            go.name = "shadowNavMesh";
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshRenderer>().material = material;
 
            go.transform.parent = GameObject.Find("NavSurfaces").transform;
            shadowNavMeshes.Add(go);
        }
        
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {   
        while (true)
        {
            if (!NightDayCycle.isNightTime)
            {
                if(shadows.Count > 0)
                {
                    agentTypeID = shadows[0].GetComponent<NavMeshAgent>().agentTypeID;
                    foreach (GameObject shadow in shadows)
                    {
                        shadow.GetComponent<NavMeshAgent>().agentTypeID = agentTypeID;
                    }
                }
                for (int k = 0; k < objs.Length; k++)
                {
                    //handle one object at a time rn, also static objects
                    vertices = objs[k].GetComponent<MeshFilter>().mesh.vertices;

                    //verticies from local to world
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = objs[k].transform.TransformPoint(vertices[i]);
                    }

                    //remove duplicate verticies created by trianges sharing edges
                    filteredVertices = RemoveDuplicates(vertices);

                    //get hit points along the ground
                    Vector3[] groundIntersections = GetHitsAlongGround(filteredVertices);

                    //returns a list of points along the convex hull
                    List<Vector3> hull = FindConvexHull(groundIntersections);
                    GeneratePolygon(hull, shadowNavMeshes[k]);
                    NavMeshBaker.GetComponent<NavMeshSurface>().BuildNavMesh();

                }
            }
            else
            {
                foreach(GameObject shadow in shadows)
                {
                    shadow.GetComponent<NavMeshAgent>().agentTypeID = 0;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    void GeneratePolygon(List<Vector3> hull, GameObject shadowCastingMesh)
    {
        Mesh mesh = new Mesh();
        Vector3[] verts = hull.ToArray();
        mesh.vertices = verts;
        List<int> tris = new List<int>();
        for(int i = 0; i< hull.Count - 2; i++)
        {
            tris.Add(0);
            tris.Add(i + 1);
            tris.Add(i + 2);
        }
        int[] triangles = tris.ToArray();
        mesh.triangles = triangles;
        shadowCastingMesh.GetComponent<MeshFilter>().mesh = mesh;

    }

    Vector3[] GetHitsAlongGround(Vector3[] verts)
    {
        Vector3[] groundIntersections  = new Vector3[filteredVertices.Length];
        for(int i = 0; i< filteredVertices.Length; i++)
        {
            if (Physics.Raycast(transform.position, filteredVertices[i] - transform.position, out hit, Mathf.Infinity,7))
            {
                groundIntersections[i] = hit.point;
                Debug.DrawRay(transform.position, hit.point - transform.position, Color.yellow);
            }
        }
        return groundIntersections;
    }

    Vector3[] RemoveDuplicates(Vector3[] verts)
    {
        HashSet<Vector3> set = new HashSet<Vector3>(verts);
        Vector3[] result = new Vector3[set.Count];
        set.CopyTo(result);
        return result;
    }

    List<Vector3> FindConvexHull( Vector3[] groundIntersections )
    {
        List<Vector3> hull = new List<Vector3>();
        Vector3 startingPoint = GetStartingPoint(groundIntersections);
        hull.Add(startingPoint);

        Vector3 imaginaryPoint = new Vector3(startingPoint.x +1, startingPoint.y, startingPoint.z);
        //imaginaryPointIndicator.transform.position = imaginaryPoint;

        Vector3 secondPoint = GetSecondPoint(imaginaryPoint,startingPoint,groundIntersections);
        hull.Add(secondPoint);

        Vector3 foundPoint = new Vector3();
        int i = 1;
        foundPoint = FindNextPoint(hull[i - 1], hull[i], groundIntersections);
        while(foundPoint != startingPoint)
        {
           foundPoint = FindNextPoint(hull[i-1],hull[i],groundIntersections);
           
           if(foundPoint != startingPoint)
           hull.Add(foundPoint);

           i++;
        }
       
        return hull;
    }

    float CheckIfCounterClockwizeAndAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector2 A = new Vector2(a.x,a.z);
        Vector2 B = new Vector2(b.x,b.z);
        Vector2 C = new Vector2(c.x,c.z);

        float angle = Vector2.Angle(A-B,C-B);
        return angle;
    }

    //get left most point
    Vector3 GetStartingPoint(Vector3[] points)
    {
        Vector3 minPoint = points[0];
        foreach (Vector3 point in points)
        {
            if (point.z < minPoint.z)
            {
                minPoint = point;
            }
        }
        return minPoint;
    }

    Vector3 GetSecondPoint(Vector3 imaginaryPoint, Vector3 startingPoint, Vector3[] points)
    {
        Vector3 secondPoint = new Vector3();
        Vector3 largestAnglePoint = new Vector3();
        float largestArea = 0;
        foreach (Vector3 point in points)
        {
            float angle;
            angle = CheckIfCounterClockwizeAndAngle(imaginaryPoint, startingPoint, point);
                if(angle > largestArea)
                {
                    largestAnglePoint = point;
                    largestArea = angle;
                }
        }
        
        secondPoint = largestAnglePoint;
        return secondPoint;
    }
    Vector3 FindNextPoint(Vector3 point1, Vector3 point2, Vector3[] points)
    {
        Vector3 largestAnglePoint = new Vector3();
        float largestArea = 0;

        foreach (Vector3 point in points)
        {
            float angle;
            angle = CheckIfCounterClockwizeAndAngle(point1, point2, point);
            if (angle > largestArea)
            {
                largestAnglePoint = point;
                largestArea = angle;
            }
        }
        return largestAnglePoint;
    }

}
