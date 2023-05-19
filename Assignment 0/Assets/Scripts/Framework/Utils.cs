using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClipperLib;

using ClipperPath = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperPaths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;



public class Utils : MonoBehaviour
{

    public const string LineGroupName = "LineVizGroup";
    public const string LineGroupLayer = "LineViz";
    public const float ZOffset = 0.01f;


    public static void AllVerticesFromPolygons(List<Polygon> polys,  out List<Vector2Int> intVertices)
    {
      
        intVertices = new List<Vector2Int>();

        foreach (var poly in polys)
        {
            var pts = poly.getIntegerPoints();
            foreach (var pt in pts)
            {
                intVertices.Add(pt);
            }
        }
    }


    public static void AllVerticesFromPolygons(List<Polygon> polys, out List<Vector2> vertices)
    {
        vertices = new List<Vector2>();
  
        foreach (var poly in polys)
        {
            var pts = poly.getPoints();
            foreach (var pt in pts)
            {
                vertices.Add(pt);
            }
        }
    }


    public static bool IsLineInPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
    {
        foreach (var poly in polys)
        {
            if (poly.IsLineSegmentOfPolygon(A, B))
                return true;
        }

        return false;
    }


    public static bool IsLineInPolygons(Vector2 A, Vector2 B, List<Polygon> polys)
    {
        foreach (var poly in polys)
        {
            if (poly.IsLineSegmentOfPolygon(A, B))
                return true;
        }

        return false;
    }

    public static bool IsLineCoincidentWithLineInPoly(Vector2 A, Vector2 B, Polygon poly)
    {
        float epsilon = 0.001f;

        return IsLineCoincidentWithLineInPoly(A, B, poly, epsilon);
    }


    public static bool IsLineCoincidentWithLineInPoly(Vector2 A, Vector2 B, Polygon poly, float epsilon)
    {
        var pts = poly.getPoints();
        for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
        {
            if (IsCollinear(A, B, pts[i], epsilon) && IsCollinear(A, B, pts[j], epsilon))
            {

                //Debug.LogError("COLLINEAR **************************");

                var dir = (B - A).normalized;

                var minL = Vector2.Dot(dir, A);
                var maxL = Vector2.Dot(dir, B);

                if (maxL < minL)
                {
                    var tmp = minL;
                    minL = maxL;
                    maxL = tmp;
                }

                var minP = Vector2.Dot(dir, pts[i]);
                var maxP = Vector2.Dot(dir, pts[j]);

                if (maxP < minP)
                {
                    var tmp = minP;
                    minP = maxP;
                    maxP = tmp;
                }
                //Debug.Log($"{minL}, {maxL} > < {minP}, {maxP}");

                // overlap test
                if (!(minL - maxP >= -epsilon || -epsilon <= minP - maxL))
                {
                    //Debug.Log("OVERLAP***************************8");
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsLineCoincidentWithLineInPolys(Vector2 A, Vector2 B, List<Polygon> polys)
    {
        float epsilon = 0.01f;

        return IsLineCoincidentWithLineInPolys(A, B, polys, epsilon);
    }


    public static bool IsLineCoincidentWithLineInPolys(Vector2 A, Vector2 B, List<Polygon> polys, float epsilon)
    {

        foreach (var poly in polys)
        {
            if (IsLineCoincidentWithLineInPoly(A, B, poly, epsilon))
                return true;
        }

        return false;
    }



    public static void GenerateOffsetNavSpace(Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
        float agentRadius,
        List<Polygon> obstaclePolys, out List<Polygon> newPolys)
    {
        //const float scale = 1000.0f;

        ClipperHelper.AssertCounterClockwise(obstaclePolys);

        ClipperPaths clipperPolys;

        ClipperHelper.PolygonsToClipper(obstaclePolys, out clipperPolys);

        ClipperPaths pUnion, pExpanded;

        ClipperHelper.ClipperUnion(clipperPolys, out pUnion);

        ClipperHelper.ClipperExpand(pUnion, agentRadius, out pExpanded);
        //ClipperHelper.ClipperExpandNoUnion(clipperPolys, agentRadius, out pExpanded);

        ClipperPaths cBoundary;

        ClipperHelper.BoundaryToClipper(canvasOrigin, canvasWidth, canvasHeight, out cBoundary);

        ClipperPaths offsetBoundary;

        ClipperHelper.ClipperExpand(cBoundary, -agentRadius, out offsetBoundary);

        ClipperPaths pFinal;

        //ClipperSubtract(offsetBoundary, pExpanded, out pFinal);

        //ClipperHelper.ClipperIntersectNoUnion(pExpanded, offsetBoundary, out pFinal);
        ClipperHelper.ClipperIntersect(pExpanded, offsetBoundary, out pFinal);

        ClipperHelper.ClipperPathsToPolyList(pFinal, out newPolys);

    }

    public static Polygon LargestPolygon(List<Polygon> polys)
    {
        Polygon largest = null;
        float largestSize = float.MinValue;

        if (polys == null)
            return null;

        for (int i = 0; i < polys.Count; ++i)
        {
            var poly = polys[i];
            Vector2 min, max;
            //poly.CalculateBounds(out min, out max);
            min = poly.MinBounds;
            max = poly.MaxBounds;

            var size = (max.x - min.x) * (max.y - min.y);

            if (size > largestSize)
            {
                largestSize = size;
                largest = poly;
            }
        }

        return largest;
    }


    //// this is an approximate test specific for use with complex polygons
    //public static bool IntersectsWithComplexPolys(Vector2 a, Vector2 b, List<Polygon> polys)
    //{
    //    const int subdivisionSteps = 10;

    //    if (polys == null)
    //        return false;

    //    bool aOnPoly = false;
    //    bool bOnPoly = false;

    //    foreach (var poly in polys)
    //    {
    //        // Phase I - basic screening

    //        var pts = poly.getPoints();
    //        if (pts == null)
    //            continue;

    //        for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
    //        {
    //            if (!aOnPoly)
    //                aOnPoly = a == pts[i];

    //            if (!bOnPoly)
    //                bOnPoly = b == pts[i];

    //            if (Intersects(a, b, pts[i], pts[j]))
    //                return true;
    //        }

    //        // Phase II - now deal with much tougher situation likely
    //        // if a and b are both vertices of the poly in question
    //        // (or line formed by a,b perfectly passes through 2 verts
    //        // of poly)

    //        Vector2 diff = b - a;
    //        float magnitude = diff.magnitude;
    //        Vector2 direction = diff.normalized;

    //        float stepSize = magnitude / (float)(subdivisionSteps + 1);

    //        for (int i = 1; i <= subdivisionSteps; ++i)
    //        {
    //            Vector2 testPos = a + direction * (i * stepSize);

    //            if (poly.IsPointInsidePolygon(testPos))
    //            {
    //                return true;
    //            }
    //        }

    //        if (!aOnPoly && poly.IsPointInsidePolygon(a))
    //            return true;

    //        if (!bOnPoly && poly.IsPointInsidePolygon(b))
    //            return true;
    //    }

    //    return false;
    //}





    public static bool Intersects(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        //check if c and d lie on opposite sides of a and b
        //check if a and b lie on opposite sides of c and d
        float detA = det((d - c), (a - c));
        float detB = det((d - c), (b - c));
        bool dirA = detA > 0;
        bool dirB = detB > 0;
        if (!(dirA ^ dirB) || detA == 0 || detB == 0) return false;
        float detC = det((b - a), (c - a));
        float detD = det((b - a), (d - a));
        bool dirC = detC > 0;
        bool dirD = detD > 0;
        if (!(dirC ^ dirD) || detC == 0 || detD == 0) return false;
        return true;
    }

    public static bool Intersects(Vector2 a, Vector2 b, Polygon poly)
    {

        if (poly == null || poly.getPoints() == null || poly.getPoints().Length < 1)
            return false;

        bool doesIntersect = false;

        for (int i = 0; i < poly.getPoints().Length; ++i)
        {
            var pt1 = poly.getPoints()[i];
            var pt2 = poly.getPoints()[(i + 1) % poly.getPoints().Length];
            if (Intersects(a, b, pt1, pt2))
            {
                doesIntersect = true;
                break;
            }
        }

        return doesIntersect;

    }

    public static float det(Vector2 a, Vector2 b)
    {
        float res = a.x * b.y - b.x * a.y;
        return res;
    }
    public static GameObject FindOrCreateGameObjectByName(string name)
    {
        var go = GameObject.Find(name);

        if (go == null)
        {
            go = new GameObject(name);
        }

        return go;
    }

    public static GameObject FindOrCreateGameObjectByName(GameObject parent, string name)
    {

        Transform xform = null;

        if (parent == null)
        {
            var tmp = GameObject.Find("/" + name);

            if (tmp != null)
                xform = tmp.transform;
        }
        else
        {
            xform = parent.transform.Find(name);
        }

        GameObject go = null;

        if (xform == null)
        {
            go = new GameObject(name);

            if (parent != null)
                go.transform.parent = parent.transform;
            else
                go.transform.parent = null;
        }
        else
        {
            go = xform.gameObject;
        }

        return go;
    }
    public static GameObject DrawLine(Vector2 start, Vector2 end, float zpos, GameObject parent, Color color, Material mat = null, float lineWidth = 0.05f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.parent = parent.transform;//FindOrCreateByName(Utils.LineGroupName).transform;
        myLine.layer = LayerMask.NameToLayer(LineGroupLayer);
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        if (mat != null)
        {
            lr.sharedMaterial = mat;
        }
        //lr.material.color = color;
        lr.startColor = color;
        lr.startWidth = lineWidth;
        lr.endColor = color;
        lr.endWidth = lr.startWidth;
        lr.SetPosition(0, new Vector3(start.x, zpos, start.y));
        lr.SetPosition(1, new Vector3(end.x, zpos, end.y));

        return myLine;
    }


    public static void DisplayName(string parent, string name)
    {
        var snameObj = GameObject.Find("StudentName");

        if (snameObj == null)
        {
            Debug.LogError("Name text field not found!");
        }
        else
        {
            var txt = snameObj.GetComponent<Text>();

            if (txt == null)
            {
                Debug.LogError("No text!");
            }
            else
            {
                txt.text += parent + ":" + name + System.Environment.NewLine;
            }
        }
    }

    //Get the shortest distance from a point to a line
    //Line is defined by the lineStart and lineEnd points
    public static float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //If point is beyond the two points of the line, return the shorter distance to the line end points
        Vector2 ptToA = lineStart - point;
        Vector2 ptToB = lineEnd - point;
        if (Vector2.Dot(ptToA, ptToB) > 0)
        {
            //point is beyond end points
            return Mathf.Min(ptToA.magnitude, ptToB.magnitude);
        }
        else
        {
            //find the perpendicular distance to line
            Vector2 AB = lineStart - lineEnd;
            AB.Normalize();
            float scale = Vector2.Dot(-ptToB, AB);
            AB.Scale(new Vector2(scale, scale));
            Vector2 pt = lineEnd + AB;
            return Vector2.Distance(pt, point);
        }
    }
    /*
     * Returns true if the point lies on the polygon.
     * Since exact distance cannot be measured we rely on a small value of epsilon
     */
    public static bool PointOnPolygon(Vector2 point, Vector2[] polygon)
    {
        float epsilon = 0.1f;
        for (int i = 0; i < polygon.Length; i++)
        {
            int j = (i + 1) % polygon.Length;
            if (DistanceToLine(point, polygon[i], polygon[j]) < epsilon)
                return true;
        }
        return false;
    }


    // Return True if the polygon is convex.
    public static bool IsConvex(Vector2[] poly)
    {
        if (poly == null || poly.Length < 3) return false;

        bool negDirSeen = false; bool posDirSeen = false;

        for (int i = 0, j = poly.Length - 1, k = poly.Length - 2;
            i < poly.Length; k = j, j = i++)
        {
            var cp = CrossProductMagnitude(poly[i], poly[j], poly[k]);

            if (cp < 0f) negDirSeen = true;
            else if (cp > 0f) posDirSeen = true;

            if (negDirSeen && posDirSeen) return false;
        }
        return true;
    }

    public static float CrossProductMagnitude(Vector2 A, Vector2 B, Vector2 C)
    {
        return (A.x - B.x) * (C.y - B.y) - (A.y - B.y) * (C.x - B.x);
    }


    /*
     * Returns false if there is an unobstructed  ray from point to dest point
     * Obstruction can be caused by the values in the lines
     */
    public static bool IsRayObstructed(Vector2 src_point, Vector2 dest_point, Vector2[,] lines)
    {
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            if (Intersects(src_point, dest_point, lines[i, 0], lines[i, 1]))
                return true;
        }
        return false;
    }

    //
    // Returns false if there is an unobstructed  ray from point to dest point
    // Obstruction can be caused by the values in the lines
    //
    //public static bool IsRayObstructed(Vector2 src_point, Vector2 dest_point, List<Polygon> polygons)
    //{
    //    for (int i = 0; i < polygons.Count; i++)
    //    {
    //        Vector2[] points = polygons[i].getPoints();
    //        int i1 = -1, i2 = -1;
    //        for (int j = 0; j < points.Length; j++)
    //        {
    //            if (src_point == points[j])
    //            {
    //                i1 = j;
    //                //both points on same polygon? We are going inside a convex polygon
    //                //
    //                /*if (i2 != -1)
    //                    if ((i2 + 1) % points.Length != i1 && (i1 + 1) % points.Length != i2)
    //                    {
    //                        //check intersection of this line against every line of the obstacle.
    //                        //this check is needed for concave polygon
    //                        for(int k = 0; k < p)
    //                        return true;
    //                    }*/
    //            }
    //            if (dest_point == points[j])
    //            {
    //                i2 = j;
    //                //both points on same polygon? We are going inside a convex polygon
    //                /*if (i1 != -1)
    //                    if ((i2 + 1) % points.Length != i1 && (i1 + 1) % points.Length != i2)
    //                        return true;
    //            */
    //            }
    //            if (Intersects(src_point, dest_point, points[j], points[(j + 1) % points.Length]))
    //                return true;
    //        }
    //        //mid point check for polygons
    //        //in case the points lie on the polygons, this test will help us figure that out
    //        //do this only in case the points are not found 
    //        if ((i1 == -1) || (i2 == -1) || (i1 != -1 && i2 != -1) && (i1 + 1) % points.Length != i2 && (i2 + 1) % points.Length != i1)
    //        {
    //            Vector2 midPoint = new Vector2(0.00f, 0.00f);

    //            midPoint.x = midPoint.x + 0.50f * (src_point.x + dest_point.x);
    //            midPoint.y = midPoint.y + 0.50f * (src_point.y + dest_point.y);
    //            if (polygons[i].IsPointInsidePolygon(midPoint))
    //                return true;
    //        }
    //    }
    //    return false;
    //}

    //
     // Returns the index of the point in nodes that is closest to the current point without
     // Being obstructed by any line in the lines tuple
     // Will return -1 if there is no such point
     ///
    //public static int FindClosestUnobstructed(Vector2 point, Vector2[] nodes, List<Polygon> polygons)
    //{
    //    float minDist = float.MaxValue;
    //    int minIndex = -1;
    //    for (int i = 0; i < nodes.Length; i++)
    //    {
    //        if (IsRayObstructed(point, nodes[i], polygons))
    //            continue;
    //        float dist = Vector2.Distance(point, nodes[i]);
    //        if (minDist > dist)
    //        {
    //            minIndex = i;
    //            minDist = dist;
    //        }
    //    }
    //    return minIndex;
    //}

    /**
     * Returns true if the two polygons are adjacent to each other 
     * Polygons are adjacent if any two sides are the same
     */
    public static bool PolygonsAdjacent(Vector2[] polygon1, Vector2[] polygon2)
    {
        float epsilon = 0.01f;
        for (int i = 0; i < polygon1.Length; i++)
        {
            int i_n = (i + 1) % polygon1.Length;
            for (int j = 0; j < polygon2.Length; j++)
            {
                int j_n = (j + 1) % polygon2.Length;
                if ((Vector2.Distance(polygon1[i], polygon2[j]) < epsilon && Vector2.Distance(polygon1[i_n], polygon2[j_n]) < epsilon)
                    || (Vector2.Distance(polygon1[i_n], polygon2[j]) < epsilon && Vector2.Distance(polygon1[i], polygon2[j_n]) < epsilon))
                    return true;
            }
        }
        return false;
    }



    /**Finds the centroid of the given polygon
     * */
    public static Vector2 GetCentroid(Vector2[] polygon)
    {
        if (polygon.Length == 0) return Vector2.zero;
        Vector2 sum = Vector2.zero;
        for (int i = 0; i < polygon.Length; i++)
        {
            sum += polygon[i];
        }
        sum = sum / polygon.Length;
        return sum;
    }

    public static Vector2[] CombineArrays(Vector2[] v1, Vector2[] v2)
    {
        int totalLength = v1.Length + v2.Length;
        Vector2[] cArray = new Vector2[totalLength];
        v1.CopyTo(cArray, 0);
        v2.CopyTo(cArray, v1.Length);
        return cArray;
    }

    public static Vector2 PerturbPoint(Vector2 p)
    {
        float epsilon = 0.01f;
        Vector2 pt = p + new Vector2(Random.Range(0, epsilon), Random.Range(0, epsilon));
        return pt;
    }



    // convex intersection test code
    //Polygon Apoly = new Polygon();
    //Polygon Bpoly = new Polygon();

    //Apoly.SetPoints(new Vector2[] {new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(1f, 1f) });

    //to the right well clear
    //Bpoly.SetPoints(new Vector2[] { new Vector2(2f, 0f), new Vector2(3f, 0f), new Vector2(3f, 1f) });

    //clearly overlapping area
    //Bpoly.SetPoints(new Vector2[] { new Vector2(0.5f, 0f), new Vector2(1.5f, 0f), new Vector2(1.5f, 1f) });

    //above but touching on edge
    //Bpoly.SetPoints(new Vector2[] { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 2f) });

    //to the right but touching on edge
    //Bpoly.SetPoints(new Vector2[] { new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f) });

    //Utils.ConvexIntersection(Apoly, Bpoly);

    static bool ConvexIntersectionHelper(Polygon A, Polygon B, Vector2[] aPts, Vector2[] bPts, float epsilon)
    {
        for (int i = 0, j = aPts.Length - 1; i < aPts.Length; j = i++)
        {
            // Debug.Log($"Considering: {aPts[j].x}, {aPts[j].y} -> {aPts[i].x}, {aPts[i].y}");

            var divLineCandidate = aPts[i] - aPts[j];
            // perpendicular to divLine
            var projLine = (new Vector2(-divLineCandidate.y, divLineCandidate.x)).normalized;

            // Debug.Log($"DivLine: {divLineCandidate.x}, {divLineCandidate.y}");
            // Debug.Log($"ProjLine: {projLine.x}, {projLine.y}");

            float minA = float.MaxValue;
            float maxA = float.MinValue;
            //project pts of A
            for (int k = 0; k < aPts.Length; ++k)
            {
                var val = Vector2.Dot(aPts[k], projLine);

                if (val < minA)
                    minA = val;

                if (val > maxA)
                    maxA = val;
            }

            // Debug.Log($"A min: {minA}, max: {maxA}");

            float minB = float.MaxValue;
            float maxB = float.MinValue;

            for (int k = 0; k < bPts.Length; ++k)
            {
                var val = Vector2.Dot(bPts[k], projLine);

                if (val < minB)
                    minB = val;

                if (val > maxB)
                    maxB = val;
            }

            // Debug.Log($"B min: {minB}, max: {maxB}");

            //float epsilon = 0.001f;
            if (minA - maxB >= -epsilon || -epsilon <= minB - maxA)
                return false;
        }

        return true;
    }

    public static bool ConvexIntersection(Polygon A, Polygon B)
    {
        float epsilon = 0.001f;
        return ConvexIntersection(A, B, epsilon);
    }

    //http://web.archive.org/web/20141127210836/http://content.gpwiki.org/index.php/Polygon_Collision
    public static bool ConvexIntersection(Polygon A, Polygon B, float epsilon)
    {
        if (A == null || B == null)
            return false;

        var aPts = A.getPoints();
        var bPts = B.getPoints();

        if (aPts == null || aPts.Length < 3 || bPts == null || bPts.Length < 3)
            return false;

        if (!ConvexIntersectionHelper(A, B, aPts, bPts, epsilon))
        {
            // Debug.Log("A had valid dividing line");
            return false;
        }

        if (!ConvexIntersectionHelper(B, A, bPts, aPts, epsilon))
        {
            // Debug.Log("B had valid dividing line");
            return false;
        }

        // Debug.Log("polygons must intersect because no dividing line found");
        return true;
    }

    public static bool ConvexIntersection(Polygon A, List<Polygon> B)
    {
        float epsilon = 0.001f;
        return ConvexIntersection(A, B, epsilon);
    }

    public static bool ConvexIntersection(Polygon A, List<Polygon> B, float epsilon)
    {
        if (A == null || B == null)
            return false;

        foreach (var p in B)
        {
            if (ConvexIntersection(A, p, epsilon))
                return true;
        }

        return false;
    }

    public static bool IsCollinear(Polygon p, float epsilon)
    {
        if (p == null)
            return false;

        var pts = p.getPoints();

        if (pts.Length < 3)
            return false;

        for (int i = 0; i <= pts.Length - 3; ++i)
        {
            if (!IsCollinear(pts[i], pts[i + 1], pts[i + 2], epsilon))
                return false;
        }

        return true;
    }

    public static bool IsCollinear(Polygon p)
    {
        float epsilon = 0.001f;
        return IsCollinear(p, epsilon);
    }

    public static bool IsCollinear(Vector2 v1, Vector2 v2, Vector2 v3, float epsilon)
    {
        var area2x = v1.x * (v2.y - v3.y) + v2.x * (v3.y - v1.y) + v3.x * (v1.y - v2.y);
        //Debug.Log($"{v1.x}, {v1.y}: {v2.x}, {v2.y}: {v3.x}, {v3.y}:: area2x: {area2x} {Mathf.Abs(area2x) < epsilon}");
        return Mathf.Abs(area2x) < epsilon;
    }

    public static bool IsCollinear(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float epsilon = 0.001f;
        return IsCollinear(v1, v2, v3, epsilon);
    }



    //// merge polygons across common edge AB
    //public static Polygon MergePolygons(Polygon poly1, Polygon poly2, Vector2 A, Vector2 B)
    //{
    //    Polygon mPoly = null;

    //    if (poly1 == null || poly2 == null)
    //        return mPoly;

    //    if (poly1.GetLength() < 3 || poly2.GetLength() < 3)
    //    {
    //        Debug.LogError("Degenerate Polygon (too few verts)");
    //        return mPoly;
    //    }

    //    //if (poly1.IsClockwise() != poly2.IsClockwise())
    //    if(CG.Ccw(poly1.getIntegerPoints()) != CG.Ccw(poly2.getIntegerPoints()))
    //    {
    //        Debug.LogError("Polygons have different windings");
    //        return mPoly;
    //    }

    //    int poly1AIndex = IndexOfPointOfPolygon(poly1, A);
    //    int poly1BIndex = IndexOfPointOfPolygon(poly1, B);
    //    int poly2AIndex = IndexOfPointOfPolygon(poly2, A);
    //    int poly2BIndex = IndexOfPointOfPolygon(poly2, B);

    //    if (poly1AIndex < 0 || poly1BIndex < 0 || poly2AIndex < 0 || poly2AIndex < 0)
    //    {
    //        Debug.LogError("Common edge not found between two polygons (one or more verts not found)");
    //        return mPoly;
    //    }

    //    if (
    //        !(Mathf.Abs(poly1AIndex - poly1BIndex) == 1 ||
    //        (poly1AIndex == 0 && poly1BIndex == poly1.GetLength() - 1) ||
    //        (poly1BIndex == 0 && poly1AIndex == poly1.GetLength() - 1)) ||
    //        !(Mathf.Abs(poly2AIndex - poly2BIndex) == 1 ||
    //        (poly2AIndex == 0 && poly2BIndex == poly2.GetLength() - 1) ||
    //        (poly2BIndex == 0 && poly2AIndex == poly2.GetLength() - 1))
    //        )
    //    {
    //        Debug.LogError("Common edge not found between two polygons (verts not adjacent)");
    //        return mPoly;
    //    }

    //    //walk from the current polygon to the first index and then switch to the second polygon and second index
    //    //till the second polygon first index, switch to first polygon second index til the end

    //    Vector2[] newPoints = new Vector2[poly1.GetLength() + poly2.GetLength() - 2];


    //    int i = 0;

    //    Vector2 lastPt = Vector2.negativeInfinity;

    //    for (int j = 0, k = poly1BIndex;
    //        j < poly1.GetLength();
    //        ++j, k = ((k + 1) % poly1.GetLength()))
    //    {
    //        var newv = poly1.getPoints()[k];
    //        if (newv != lastPt)
    //        {
    //            lastPt = newv;
    //            newPoints[i] = newv;
    //            ++i;
    //        }
    //    }

    //    for (int j = 0, k = (poly2AIndex + 1) % poly2.GetLength();
    //        j < poly2.GetLength() - 2;
    //        ++j, k = ((k + 1) % poly2.GetLength()))
    //    {
    //        var newv = poly2.getPoints()[k];
    //        if (newv != lastPt)
    //        {
    //            lastPt = newv;
    //            newPoints[i] = newv;
    //            ++i;
    //        }
    //    }

    //    //DEBUG CODE
    //    //for(int m = 0; m < newPoints.Length; ++m)
    //    //{
    //    //    for(int n = m+1; n < newPoints.Length; ++n)
    //    //    {
    //    //        if(newPoints[m] == newPoints[n])
    //    //        {
    //    //            Debug.LogError("Dupe points found");
    //    //        }
    //    //    }
    //    //}

    //    mPoly = new Polygon();
    //    mPoly.SetPoints(newPoints);

    //    return mPoly;
    //}



    // merge polygons across common edge AB
    public static Polygon MergePolygons(Polygon poly1, Polygon poly2, Vector2Int A, Vector2Int B)
    {
        Polygon mPoly = null;

        if (poly1 == null || poly2 == null)
            return mPoly;


        var poly1pts = poly1.getIntegerPoints();
        var poly2pts = poly2.getIntegerPoints();

        var poly1len = poly1pts.Length;
        var poly2len = poly2pts.Length;



        if (poly1len < 3 || poly2len < 3)
        {
            Debug.LogError("Degenerate Polygon (too few verts)");
            return mPoly;
        }

        //if (poly1.IsClockwise() != poly2.IsClockwise())
        if (CG.Ccw(poly1pts) != CG.Ccw(poly2pts))
        {
            Debug.LogError("Polygons have different windings");
            return mPoly;
        }

        int poly1AIndex = IndexOfPointOfPolygon(poly1pts, A);
        int poly1BIndex = IndexOfPointOfPolygon(poly1pts, B);
        int poly2AIndex = IndexOfPointOfPolygon(poly2pts, A);
        int poly2BIndex = IndexOfPointOfPolygon(poly2pts, B);

        if (poly1AIndex < 0 || poly1BIndex < 0 || poly2AIndex < 0 || poly2AIndex < 0)
        {
            Debug.LogError("Common edge not found between two polygons (one or more verts not found)");
            return mPoly;
        }



        if (
            !(
                Mathf.Abs(poly1AIndex - poly1BIndex) == 1 || // side by side in array (either order)
                (poly1AIndex == 0 && poly1BIndex == poly1len - 1) || // on either end (AB)
                (poly1BIndex == 0 && poly1AIndex == poly1len - 1) // on either end (BA)
              ) || 
            !(
                Mathf.Abs(poly2AIndex - poly2BIndex) == 1 || // side by side in array (either order)
                (poly2AIndex == 0 && poly2BIndex == poly2len - 1) || // on either end (AB)
                (poly2BIndex == 0 && poly2AIndex == poly2len - 1) // on either end (BA)
             )
           )
        {
            Debug.LogError($"Common edge not found between two polygons (verts not adjacent): poly1_count: {poly1len} poly2_count: {poly2len} A: {A} B: {B}");

            Debug.Log($"poly1: {poly1}");
            Debug.Log($"poly2: {poly2}");

            return mPoly;
        }

        //walk from the current polygon to the first index and then switch to the second polygon and second index
        //till the second polygon first index, switch to first polygon second index til the end

        Vector2Int[] newPoints = new Vector2Int[poly1len + poly2len - 2];


        int i = 0;

        Vector2Int lastPt = new Vector2Int(int.MinValue, int.MinValue);



        for (int j = 0, k = poly1BIndex; // Clockwise version had poly1BIndex
            j < poly1len;
            ++j, k = ((k + 1) % poly1len))
        {
            var newv = poly1pts[k];
            if (newv != lastPt)
            {
                lastPt = newv;
                newPoints[i] = newv;
                ++i;
            }
        }

        for (int j = 0, k = (poly2AIndex + 1) % poly2len; //Clockwise version had poly2AIndex
            j < poly2len - 2;
            ++j, k = ((k + 1) % poly2len))
        {
            var newv = poly2pts[k];
            if (newv != lastPt)
            {
                lastPt = newv;
                newPoints[i] = newv;
                ++i;
            }
        }



        ////DEBUG CODE
        for (int m = 0; m < newPoints.Length; ++m)
        {
            for (int n = m + 1; n < newPoints.Length; ++n)
            {
                if (newPoints[m] == newPoints[n])
                {
                    Debug.LogError($"Dupe points found: {newPoints[m]}");
                }
            }
        }

        mPoly = new Polygon();
        mPoly.SetIntegerPoints(newPoints);


        //Debug.Log($"New poly is size: {newPoints.Length}");

        return mPoly;
    }




    public static int IndexOfPointOfPolygon(Vector2Int[] poly, Vector2Int p)
    {
        int ret = -1;

        if (poly == null)
            return ret;

        var pts = poly;

        for (int i = 0; i < pts.Length; ++i)
        {
            if (pts[i] == p)
                return i;
        }

        return ret;
    }


    //public static int IndexOfPointOfPolygon(Polygon poly, Vector2 p)
    //{
    //    int ret = -1;

    //    if (poly == null)
    //        return ret;

    //    var pts = poly.getPoints();

    //    for (int i = 0; i < pts.Length; ++i)
    //    {
    //        if (pts[i] == p)
    //            return i;
    //    }

    //    return ret;
    //}
}