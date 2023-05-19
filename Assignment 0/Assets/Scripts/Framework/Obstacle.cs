using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IDragFinishedObserver
{
    Polygon polygon = new Polygon();
    public Material LineMaterial;
    public Color LineColor = Color.red;
    public Vector2[] initPoints;
    //MeshFilter meshFilter;

    private void Awake()
    {
        //Eventually use this to work with any mesh (maybe just convex)
        //meshFilter = GetComponent<MeshFilter>();

        //if (meshFilter == null)
        //    Debug.LogError("No Mesh Filter");

        

    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        polygon.Init();

        if (initPoints != null && initPoints.Length > 0)
        {

            Vector2[] xformPts = new Vector2[initPoints.Length];

            int i = 0;
            foreach (var pt in initPoints)
            {
                Vector3 pt3d = new Vector3(pt.x, 0f, pt.y);
                var xpt3d = this.transform.TransformPoint(pt3d);
                Vector2 xpt = new Vector2(xpt3d.x, xpt3d.z);
                xformPts[i] = xpt;
                ++i;
            }

            polygon.SetPoints(xformPts);

            if(!CG.Ccw(polygon.getIntegerPoints()))
            {

                Debug.LogError("Polygon found with NOT CCW. Fixing!");
                polygon.Reverse();
            }
        }
        DrawPolygon();
    }


    void PurgeOutdatedLineViz()
    {

        var linegroup = this.transform.Find(Utils.LineGroupName);

        if (linegroup != null)
        {
            linegroup.name = "MARKED_FOR_DELETION";
            linegroup.gameObject.SetActive(false);
            Destroy(linegroup.gameObject);
        }
    }

    public void SetPoints(Vector2[] newPoints)
    {

        polygon.SetPoints(newPoints);

        SetInitPointsFromPoly(polygon);
    }

    public void DrawPolygon()
    {
        PurgeOutdatedLineViz();

        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);

        for (int i = 0; i < GetPoints().Length; i++)
        {
            Utils.DrawLine(GetPoints()[i], GetPoints()[(i+1)% GetPoints().Length], Utils.ZOffset+0.03f, parent, LineColor, LineMaterial);
        }
    }

    public Polygon GetPolygon()
    {
        return polygon;
    }

    void SetInitPointsFromPoly(Polygon p)
    {
        initPoints = new Vector2[p.getPoints().Length];

        for (int i = 0; i < p.getPoints().Length; ++i)
        {
            initPoints[i] = p.getPoints()[i];
        }
    }

    public void SetPolygon(Polygon p)
    {
        polygon = p;

        SetInitPointsFromPoly(p);
    }

    public Vector2 GetCentroid()
    {
        return polygon.GetCentroid();
    }

    //public Vector2[,] GetLines()
    //{
    //    return polygon.GetLines();
    //}

    public Vector2Int[] GetIntegerPoints()
    {
        return polygon.getIntegerPoints();
    }


    public Vector2[] GetPoints()
    {
        return polygon.getPoints();
    }


    public bool IsPointInPolygon(Vector2 p)
    {
        //return polygon.IsPointInsidePolygon(p);
        return CG.InPoly1(GetPoints(), p) != CG.PointPolygonIntersectionType.Outside;
    }

    public bool IsLineInPolygon(Vector2 ptA, Vector2 ptB)
    {
        return polygon.IsLineSegmentOfPolygon(ptA, ptB);

    }

    public int GetLength()
    {
        return GetPoints().Length;
    }
    ////
    // // Returns if the polygon is clockwise
    // // Will only work for convex polygons
    // // 
    //public bool IsClockwise()
    //{
    //    return polygon.IsClockwise();
    //}

    /*
     * Reverses the direction of this polygon
     */
    public void Reverse()
    {
        polygon.Reverse();
    }

    public void SetPosition(Vector2 p)
    {
        this.transform.position = new Vector3(p.x, 0, p.y);
    }

    public void OnDragFinished()
    {
        Init();   
    }
}
