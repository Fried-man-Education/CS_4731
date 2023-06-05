using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacentPolygons : Dictionary<CommonPolygonEdge, CommonPolygons>
{
    public AdjacentPolygons() : base()
    {

    }

    public AdjacentPolygons(AdjacentPolygons ap) : base(ap)
    {

    }

    public void AddPolygon(Polygon p)
    {
        AddPolygon(p, null, null);
    }

    public void AddPolygon(Polygon p, Polygon replacePolyA, Polygon replacePolyB)
    {
        if (p == null)
            return;

        var pts = p.getIntegerPoints();
        var ptslen = pts.Length;

        for (int i = 0, j = ptslen - 1; i < ptslen; j = i++)
        {
            var cpe = new CommonPolygonEdge(pts[j], pts[i]);


            if (!this.ContainsKey(cpe))
            {
                this.Add(cpe, new CommonPolygons(cpe, p));
            }
            else
            {
                var currcp = this[cpe];

                int clearSpots = 0;

                if (replacePolyA != null)
                {
                    if (currcp.AB == replacePolyA)
                    {
                        currcp.ClearABPolygon();
                        ++clearSpots;
                    }
                    else if (currcp.BA == replacePolyA)
                    {
                        currcp.ClearBAPolygon();
                        ++clearSpots;
                    }

                }
                else
                    ++clearSpots;

                if (replacePolyB != null)
                {
                    if (currcp.AB == replacePolyB)
                    {
                        currcp.ClearABPolygon();
                        ++clearSpots;
                    }
                    else if (currcp.BA == replacePolyB)
                    {
                        currcp.ClearBAPolygon();
                        ++clearSpots;
                    }

                }
                else
                    ++clearSpots;

                if (clearSpots <= 0)
                {
                    Debug.LogError($"Failed to add poly! replacePolyA null? {replacePolyA == null} replacePolyB null? {replacePolyB == null}");
                }
                else
                {
                    currcp.Add(p);
                }
            }
        }
    }

} //class
