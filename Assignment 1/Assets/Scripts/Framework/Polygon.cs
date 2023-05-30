using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : IEquatable<Polygon>
{
    Vector2[] points;       //list of all points in the polygon
    Vector2Int[] intPoints;
    Vector2[,] lines;
    Vector2 minBounds, maxBounds;
    Vector2Int minIntBounds, maxIntBounds;
    Vector2 centroid;
    Vector2Int intCentroid;


    public Vector2 MinBounds
    {
        get { return minBounds; }
        private set { minBounds = value; }
    }

    public Vector2Int MinIntBounds
    {
        get { return minIntBounds; }
        private set { minIntBounds = value; }

    }

    public Vector2 MaxBounds
    {
        get { return maxBounds;  }
        private set { maxBounds = value; }
    }

    public Vector2Int MaxIntBounds
    {
        get
        {
            return maxIntBounds;
        }
        private set { maxIntBounds = value; }
    }

    public void Init()
    {
        const float d = 1f;
        if (points == null) points = new Vector2[] { new Vector2(d, 0f), new Vector2(d, d), new Vector2(0f, 0f) };
        CreateIntPointsFromPoints();
        CalculateBounds();
        
        CreateLines();
        CalculateCentroid();
    }
    public void SetPoints(Vector2[] newPoints)
    {
        points = newPoints;
        CreateIntPointsFromPoints();
        CalculateBounds();
        CreateLines();
        CalculateCentroid();

    }

    public void SetIntegerPoints(Vector2Int[] newPoints)
    {
        intPoints = newPoints;
        CreatePointsFromIntPoints();
        CalculateBounds();
        CreateLines();
        CalculateCentroid();
    }


    void CalculateCentroid()
    {
        //centroid = Utils.GetCentroid(getPoints());
        CG.Vector2Double v;

        CG.FindCG(this.intPoints, out v);

        this.centroid = new Vector2((float)(v.x / (double)CG.FloatToIntFactor), (float)(v.y / (double)CG.FloatToIntFactor));
        this.intCentroid = new Vector2Int((int)System.Math.Round(v.x), (int)System.Math.Round(v.y));
    }


    void CreatePointsFromIntPoints()
    {
        points = CG.Convert(intPoints);
    }


    void CreateIntPointsFromPoints()
    {
        intPoints = CG.Convert(points);
    }

    void CreateLines()
    {
        lines = new Vector2[points.Length, 2];
        for (int i = 0; i < points.Length; i++)
        {
            lines[i, 0] = points[i];
            lines[i, 1] = points[(i + 1) % points.Length];
        }
    }
    public Vector2 GetCentroid()
    {
        return centroid;
    }

    public Vector2Int GetIntCentroid()
    {
        return intCentroid;
    }



    public Vector2Int[] getIntegerPoints()
    {
        return intPoints;
    }


    public Vector2[] getPoints()
    {
        return points;
    }


    protected void CalculateBounds()
    {
        bool first_point = true;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].x < minBounds.x || first_point)
            {
                minBounds.x = points[i].x;
                minIntBounds.x = intPoints[i].x;
            }
            if (points[i].x > maxBounds.x || first_point)
            {
                maxBounds.x = points[i].x;
                maxIntBounds.x = intPoints[i].x;
            }
            if (points[i].y < minBounds.y || first_point)
            {
                minBounds.y = points[i].y;
                minIntBounds.y = intPoints[i].y;
            }
            if (points[i].y > maxBounds.y || first_point)
            {
                maxBounds.y = points[i].y;
                maxIntBounds.y = intPoints[i].y;
            }
            first_point = false;
        }
    }




    // Check if a point is one of the vertices
    // If the coordinates of p are derived separately that the poly's vertices
    // then this will possibly fail due to float equality test
    public bool IsPointAVertex(Vector2Int p)
    {
        foreach (var pt in intPoints)
        {
            if (p == pt)
                return true;
        }

        return false;
    }


    public bool IsPointAVertex(Vector2 p)
    {
        foreach (var pt in getPoints())
        {
            if (p == pt)
                return true;
        }

        return false;
    }



    public bool IsLineSegmentOfPolygonSameDirection(Vector2 ptA, Vector2 ptB)
    {
        var len = points.Length;

        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            if (ptB == points[i] && ptA == points[j])
                return true;
        }
        return false;
    }



    public bool IsLineSegmentOfPolygonSameDirection(Vector2Int ptA, Vector2Int ptB)
    {
        var len = intPoints.Length;

        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            if (ptA == intPoints[j] && ptB == intPoints[i])
                return true;
        }
        return false;
    }


    public bool IsLineSegmentOfPolygonOppositeDirection(Vector2 ptA, Vector2 ptB)
    {
        var len = points.Length;

        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            if (ptA == points[i] && ptB == points[j])
                return true;
        }
        return false;
    }


    public bool IsLineSegmentOfPolygonOppositeDirection(Vector2Int ptA, Vector2Int ptB)
    {
        var len = intPoints.Length;

        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            if (ptB == intPoints[j] && ptA == intPoints[i])
                return true;
        }
        return false;
    }


    public bool IsLineSegmentOfPolygon(Vector2 ptA, Vector2 ptB)
    {
        var len = points.Length;
        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            if ((ptA == points[i] && ptB == points[j]) ||
                    (ptB == points[i] && ptA == points[j]))
                return true;
        }
        return false;
    }


    public bool IsLineSegmentOfPolygon(Vector2Int ptA, Vector2Int ptB)
    {
        var len = intPoints.Length;
        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            var ptC = intPoints[j];
            var ptD = intPoints[i];

            if ((ptA == ptC && ptB == ptD) ||
                    (ptB == ptC && ptA == ptD))
                return true;
        }
        return false;
    }


    public int GetLength()
    {
        return getIntegerPoints().Length;
    }

    //
    // Reverses the direction of this polygon
    //
    public void Reverse()
    {
        //System.Array.Reverse(this.points);
        //CreateIntPointsFromPoints();
        System.Array.Reverse(this.intPoints);
        CreatePointsFromIntPoints();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Polygon);
    }

    public bool Equals(Polygon other)
    {
        return other != null &&
                CircularBufferEquals(intPoints, other.intPoints);
               //EqualityComparer<Vector2[]>.Default.Equals(points, other.points);
    }

    static bool CircularBufferEquals(Vector2Int[] A, Vector2Int[] B)
    {
        if (A == null && B == null)
            return true;
        if (A == null)
            return false;
        if (B == null)
            return false;

        if (A.Length != B.Length)
            return false;

        if (A.Length == 0 && B.Length == 0)
            return true;

        if (B.Length == 0)
            return false;

        int i;
        for (i = 0; i < A.Length; ++i)
        {
            if (A[i] == B[0])
            {
                 break;
            }
        }

        if (i >= A.Length)
            return false;

        for(int j = i, k = 0; k < B.Length; j = (j + 1) % A.Length, ++k)
        {
            if (A[j] != B[k])
                return false;
        }

        return true;
    }

    static float SumDot(Vector2Int[] pts)
    {
        if (pts == null)
            return 0f;

        Vector2 tot = Vector2.zero;

        foreach (var p in pts)
            tot += CG.Convert(p);

        return Vector2.Dot(Vector2.right, tot);
    }

    public override int GetHashCode()
    {
        var hashCode = 1410917715;
        hashCode = hashCode * -1521134295 + EqualityComparer<float>.Default.GetHashCode(SumDot(intPoints));
        //hashCode = hashCode * -1521134295 + EqualityComparer<Vector2[]>.Default.GetHashCode(points);

        return hashCode;
    }



    public override string ToString()
    {
        string s = "";


        foreach(var p in intPoints)
        {
            s += $"{p.ToString()}, ";
        }


        return s;

    }
}
