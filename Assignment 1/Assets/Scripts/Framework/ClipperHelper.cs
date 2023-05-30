using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

using ClipperPath = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperPaths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;



public class ClipperHelper
{

    //static float scale = 1000.0f;


    public static void AssertCounterClockwise(List<Polygon> polys)
    {
        foreach (var p in polys)
        {
            Debug.Assert(CG.Ccw(p.getIntegerPoints()), "Poly not ccw");
        }
    }



    public static void ClipperPathsToPolyList(ClipperPaths cPaths, out List<Polygon> newPolys)
    {
        newPolys = new List<Polygon>(cPaths.Count);

        //Debug.Log("Poly count: " + cPaths.Count);

        foreach (var s in cPaths)
        {
            var poly = new Polygon();
            Vector2Int[] polyPts = new Vector2Int[s.Count];

            for (int p = 0; p < s.Count; ++p)
            {
                //Debug.Log("point count: " + s.Count);

                polyPts[p] = new Vector2Int((int)s[p].X , (int)s[p].Y );
            }

            poly.SetIntegerPoints(polyPts);

            newPolys.Add(poly);

        }
    }


    //public static void ClipperPathsToPolyList(ClipperPaths cPaths, out List<Polygon> newPolys)
    //{
    //    newPolys = new List<Polygon>(cPaths.Count);

    //    //Debug.Log("Poly count: " + cPaths.Count);

    //    foreach (var s in cPaths)
    //    {
    //        var poly = new Polygon();
    //        Vector2[] polyPts = new Vector2[s.Count];

    //        for (int p = 0; p < s.Count; ++p)
    //        {
    //            //Debug.Log("point count: " + s.Count);

    //            polyPts[p] = new Vector2(s[p].X / scale, s[p].Y / scale);
    //        }

    //        poly.SetPoints(polyPts);

    //        newPolys.Add(poly);

    //    }
    //}


    public static void PolygonsToClipper(List<Polygon> polys, out ClipperPaths cpaths)
    {
        cpaths = new ClipperPaths(polys.Count);

        AssertCounterClockwise(polys);

        for (int i = 0; i < polys.Count; ++i)
        {
            var pts = polys[i].getIntegerPoints();

            cpaths.Add(new ClipperPath(pts.Length));

            for (int j = 0; j < pts.Length; ++j)
            {
                cpaths[i].Add(new IntPoint(pts[j].x, pts[j].y));
            }
        }

    }


    //public static void PolygonsToClipper(List<Polygon> polys, out ClipperPaths cpaths)
    //{
    //    cpaths = new ClipperPaths(polys.Count);

    //    AssertCounterClockwise(polys);

    //    for (int i = 0; i < polys.Count; ++i)
    //    {
    //        var pts = polys[i].getPoints();

    //        cpaths.Add(new ClipperPath(pts.Length));

    //        for (int j = 0; j < pts.Length; ++j)
    //        {
    //            cpaths[i].Add(new IntPoint(Mathf.RoundToInt(scale * pts[j].x),
    //                Mathf.RoundToInt(scale * pts[j].y)));
    //        }
    //    }

    //}




    public static void ClipperUnion(ClipperPaths polys, out ClipperPaths polyUnion)
    {

        polyUnion = new ClipperPaths();

        foreach (var poly in polys)
        {
            ClipperPaths obstToJoin = new ClipperPaths();

            obstToJoin.Add(new ClipperPath(poly.Count));

            foreach (var p in poly)
            {
                obstToJoin[0].Add(p);
            }

            Clipper cunion = new Clipper();
            cunion.AddPaths(polyUnion, PolyType.ptSubject, true);
            cunion.AddPaths(obstToJoin, PolyType.ptClip, true);

            ClipperPaths tempUnionOutput = new ClipperPaths();

            cunion.Execute(ClipType.ctUnion, tempUnionOutput, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            polyUnion = tempUnionOutput;
        }

    }

    public static void ClipperExpandNoUnion(ClipperPaths polys, float offset, out ClipperPaths newPolys)
    {

        newPolys = new ClipperPaths();

        foreach (var pu in polys)
        {
            ClipperOffset coffset = new ClipperOffset();

            coffset.ArcTolerance = 25.0;

            //normally this is jtRound
            coffset.AddPath(pu, JoinType.jtRound, EndType.etClosedPolygon);

            ClipperPaths offsetPolys = new ClipperPaths();
            //coffset.Execute(ref offsetPolys, Mathf.RoundToInt(Mathf.RoundToInt(scale * offset)));
            coffset.Execute(ref offsetPolys, Mathf.RoundToInt(Mathf.RoundToInt(CG.FloatToIntFactor * offset)));

            newPolys.AddRange(offsetPolys);
        }

    }

    public static void ClipperExpand(ClipperPaths polys, float offset, out ClipperPaths newPolys)
    {
        ClipperOffset coffset = new ClipperOffset();

        coffset.ArcTolerance = 25.0;

        foreach (var pu in polys)
        {
            //normally this is jtRound
            coffset.AddPath(pu, JoinType.jtRound, EndType.etClosedPolygon);

        }

        newPolys = new ClipperPaths();

        //coffset.Execute(ref newPolys, Mathf.RoundToInt(Mathf.RoundToInt(scale * offset)));
        coffset.Execute(ref newPolys, Mathf.RoundToInt(CG.FloatToIntFactor * offset));
    }


    public static void BoundaryToClipper(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, out ClipperPaths cBoundary)
    {
        var BL = canvasOrigin;
        var TL = canvasOrigin + new Vector2(0f, canvasHeight);
        var TR = canvasOrigin + new Vector2(canvasWidth, canvasHeight);
        var BR = canvasOrigin + new Vector2(canvasWidth, 0f);

        var scale = CG.FloatToIntFactor;

        cBoundary = new ClipperPaths(1);
        cBoundary.Add(new ClipperPath(4));
        cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * BL.x),
        Mathf.RoundToInt(scale * BL.y)));
        cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * TL.x),
                Mathf.RoundToInt(scale * TL.y)));
        cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * TR.x),
                Mathf.RoundToInt(scale * TR.y)));
        cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * BR.x),
                Mathf.RoundToInt(scale * BR.y)));
        //cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * BL.x),
        //        Mathf.RoundToInt(scale * BL.y)));
        //cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * BR.x),
        //        Mathf.RoundToInt(scale * TL.y)));
        //cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * TR.x),
        //        Mathf.RoundToInt(scale * TR.y)));
        //cBoundary[0].Add(new IntPoint(Mathf.RoundToInt(scale * TL.x),
        //        Mathf.RoundToInt(scale * BR.y)));
    }


    public static void ClipperSubtract(ClipperPaths polys, ClipperPaths stencilPolys, out ClipperPaths resultPolys)
    {
        resultPolys = new ClipperPaths();

        Clipper cunion = new Clipper();
        cunion.AddPaths(polys, PolyType.ptSubject, true);
        cunion.AddPaths(stencilPolys, PolyType.ptClip, true);

        cunion.Execute(ClipType.ctDifference, resultPolys, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
    }



    public static void ClipperIntersectNoUnion(ClipperPaths polys, ClipperPaths clipPolys, out ClipperPaths resultPolys)
    {
        resultPolys = new ClipperPaths();


        foreach (var pu in polys)
        {

            Clipper cunion = new Clipper();
            cunion.AddPath(pu, PolyType.ptSubject, true);
            cunion.AddPaths(clipPolys, PolyType.ptClip, true);
            ClipperPaths temp = new ClipperPaths();
            cunion.Execute(ClipType.ctIntersection, temp, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            resultPolys.AddRange(temp);
        }
    }


    public static void ClipperIntersect(ClipperPaths polys, ClipperPaths clipPolys, out ClipperPaths resultPolys)
    {
        resultPolys = new ClipperPaths();

        Clipper cunion = new Clipper();
        cunion.AddPaths(polys, PolyType.ptSubject, true);
        cunion.AddPaths(clipPolys, PolyType.ptClip, true);

        cunion.Execute(ClipType.ctIntersection, resultPolys, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
    }


}
