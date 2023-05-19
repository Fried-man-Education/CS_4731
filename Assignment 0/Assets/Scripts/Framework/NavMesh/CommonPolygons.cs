using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CommonPolygons
{
    public Polygon AB { get; private set; }
    public Polygon BA { get; private set; }
    public CommonPolygonEdge CommonEdge { get; protected set; }
    public bool IsBarrier { get => ((AB == null) ^ (BA == null));  }

    public void ClearABPolygon()
    {
        AB = null;
    }

    public void ClearBAPolygon()
    {
        BA = null;
    }

    public void ClearPolygons()
    {
        ClearABPolygon();
        ClearBAPolygon();
    }

    bool IsEdgeInPolygon(Polygon p1)
    {
        if (p1 == null)
            return false;

        return p1.IsLineSegmentOfPolygon(CommonEdge.A, CommonEdge.B);
    }

    // Undefined if commonEdge is not in polygon
    bool IsAB(Polygon p1)
    {
        if (p1 == null)
            return false;

        return p1.IsLineSegmentOfPolygonSameDirection(CommonEdge.A, CommonEdge.B);
    }



    bool IsBA(Polygon p1)
    {
        if (p1 == null)
            return false;

        return p1.IsLineSegmentOfPolygonSameDirection(CommonEdge.B, CommonEdge.A);
    }


    // This method has some code that is now pretty crazy from an old attempt to work
    // with floating point nav mesh building. It is now much more robust and all the
    // degenerate if-clauses *should* get skipped. The degenerate stuff probably
    // needs to be deleted!
    // That said, the whole reason the code is their is because a degenerate triangle
    // can be flipped AB or BA direction pretty easily. This allows degenerates to
    // "glue" paths together that would otherwise not form.

    public void Add(Polygon p1)
    {
        if (p1 == null)
            throw new ArgumentException("Polygon cannot be null. Clear[AB/BA]Polygon[s]() instead");

        if (!IsEdgeInPolygon(p1))
            throw new ArgumentException("Polygon does not contain edge");

        //var p1IsDegenerate = Utils.IsCollinear(p1);
        var p1IsDegenerate = CG.Collinear(p1.getIntegerPoints());
        var ABIsDegenerate = false;
        var BAIsDegenerate = false;

        if (AB != null)
        {
            //ABIsDegenerate = Utils.IsCollinear(AB);
            ABIsDegenerate = CG.Collinear(AB.getIntegerPoints());
        }

        if (BA != null)
        {
            //BAIsDegenerate = Utils.IsCollinear(BA);
            BAIsDegenerate = CG.Collinear(BA.getIntegerPoints());
        }

        if (AB != null && BA != null)
        {
            if (!ABIsDegenerate && !BAIsDegenerate)
            {
                if(p1IsDegenerate)
                {
                    Debug.LogError("p1IsDegenerate and no room. Skipping");
                }
                else
                    throw new OverflowException("Two polygons already set");
            }
        }

        var isP1ABDir = IsAB(p1);
        var isP1BADir = IsBA(p1);

        if(isP1ABDir)
        {
            if (AB != null)
            {
                if (p1IsDegenerate && BA == null)
                {
                    BA = p1;
                }
                else if (ABIsDegenerate && BA == null)
                {
                    BA = AB;
                    AB = p1;
                }
                else if (BA == null)
                {
                    BA = p1;
                }
                else if (ABIsDegenerate)
                {
                    AB = p1;
                }
                else if (BAIsDegenerate)
                {
                    BA = p1;
                }
                else
                {
                    if (p1IsDegenerate)
                    {
                        Debug.LogError("attempt to add degenerate triangle but it doesn't fit");
                        //throw new ArgumentException("attempt to add degenerate triangle but it doesn't fit");
                    }
                    else
                        throw new ArgumentException("Polygon is on AB side of edge, but a AB polygon is already set");
                }
            }
            else
            {
                AB = p1;
            }
        }
        else if(isP1BADir)
        {
            if (BA != null)
            {

                if (p1IsDegenerate && AB == null)
                {
                    AB = p1;
                }
                else if (BAIsDegenerate && AB == null)
                {
                    AB = BA;
                    BA = p1;
                }
                else if(AB == null)
                {
                    AB = p1;
                }
                else if (BAIsDegenerate)
                {
                    BA = p1;
                }
                else if (ABIsDegenerate)
                {
                    AB = p1;
                }
                else
                {
                    if (p1IsDegenerate)
                    {
                        Debug.LogError("attempt to add degenerate triangle but it doesn't fit");
                        //throw new ArgumentException("attempt to add degenerate triangle, but it doesn't fit");
                    }
                    else
                        throw new ArgumentException("Polygon is on BA side of edge, but a BA polygon is already set");
                }
            }
            else
            {
                BA = p1;
            }
        }
        else
        {
            throw new ArgumentException("Polygon does not contain edge (even though we think we checked already)");
        }


    }

    public CommonPolygons(CommonPolygonEdge commonEdge)
    {
        if (commonEdge == null)
            throw new ArgumentNullException("commonEdge cannot be null");

        CommonEdge = commonEdge;
    }

    public CommonPolygons(CommonPolygonEdge commonEdge, Polygon p1):
        this(commonEdge)
    {
        Add(p1);
    }

    public CommonPolygons(CommonPolygonEdge commonEdge, Polygon p1, Polygon p2):
        this(commonEdge, p1)
    {
        Add(p2);
    }
}
