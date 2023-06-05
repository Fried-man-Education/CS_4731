using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CG
{

    // A partial port of code from Joseph O'Rourke Computational Geometry in C (2nd Ed)
    // Ported from the Java code at: http://www.science.smith.edu/~jorourke/books/ftp.html
    // Port by Dr. Jeff Wilson (jeff@imtc.gatech.edu)



    protected static int m_FloatToIntFactor = 1000;

    static public int FloatToIntFactor
    {
        get
        {
            return m_FloatToIntFactor;
        }
        private set
        {
            m_FloatToIntFactor = value;
        }
    }

    static public int Convert(float v)
    {
        return Convert(v, FloatToIntFactor);
    }

    static public int Convert(float v, int factor)
    {
        return Mathf.RoundToInt(v * factor);
    }


    static public Vector2Int Convert(Vector2 v)
    {
        return Convert(v, FloatToIntFactor);
    }

    static public Vector2Int Convert(Vector2 v, int factor)
    {
        float testV = (float)int.MaxValue / factor;
        Debug.Assert(Mathf.Abs(v.x) <= testV && Mathf.Abs(v.y) <= testV, $"Overflow if {v} is mapped to int by factor: {factor}");

        return new Vector2Int(Mathf.RoundToInt(v.x * factor), Mathf.RoundToInt(v.y * factor));
    }

    static public Vector2 Convert(Vector2Int v)
    {
        return Convert(v, FloatToIntFactor);
    }

    static public Vector2 Convert(Vector2Int v, int factor)
    {
        float f = (float)factor;

        return new Vector2(v.x / f, v.y / f);
    }


    static public Vector2Int[] Convert(Vector2[] v)
    {
        return Convert(v, FloatToIntFactor);
    }


    static public Vector2Int[] Convert(Vector2[] v, int factor)
    {
        Vector2Int[] vi = new Vector2Int[v.Length];

        for (int i = 0; i < vi.Length; ++i)
        {
            vi[i] = Convert(v[i], factor);
        }

        return vi;
    }


    static public Vector2[] Convert(Vector2Int[] vi)
    {
        return Convert(vi, FloatToIntFactor);
    }


    static public Vector2[] Convert(Vector2Int[] vi, int factor)
    {
        Vector2[] v = new Vector2[vi.Length];

        for (int i = 0; i < v.Length; ++i)
        {
            v[i] = Convert(vi[i], factor);
        }

        return v;
    }


    //----------------------------------------------------------------------------
    //Class cPointd  -- point with double coordinates
    //
    //PrintPoint() -- prints point to the console;
    //
    //---------------------------------------------------------------------------
    public class Vector2Double
    {
        public double x { get; set; }
        public double y { get; set; }

        public Vector2Double()
        {
            x = y = 0.0;
        }

        public Vector2Double(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Double(int x, int y)
        {
            this.x = (double)x;
            this.y = (double)y;
        }

        public void Assign(Vector2Int v)
        {
            x = (double)v.x;
            y = (double)v.y;
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }



    static public bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return Intersect(a, b, c, d, FloatToIntFactor);
    }


    static public bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int factor)
    {
        var ai = Convert(a, factor);
        var bi = Convert(b, factor);
        var ci = Convert(c, factor);
        var di = Convert(d, factor);

        return Intersect(ai, bi, ci, di);
    }


    // ---------------------------------------------------------------------
    // Returns TRUE iff segments ab & cd intersect, properly or improperly.
    //
    // TODO: Why not use the IntersectProp() impl and just discard the Collinear()
    // tests? Seems that would get same result and be more efficient.
    static public bool Intersect(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
    {

        //return Xor(Left(a, b, c), Left(a, b, d)) && Xor(Left(c, d, a), Left(c, d, b));

        if (IntersectProp(a, b, c, d))
            return true;

        else if (Between(a, b, c)
             || Between(a, b, d)
             || Between(c, d, a)
             || Between(c, d, b))
            return true;

        else
            return false;
    }



    static public bool IntersectProp(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return IntersectProp(a, b, c, d, FloatToIntFactor);
    }

    static public bool IntersectProp(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int factor)
    {
        var ai = Convert(a, factor);
        var bi = Convert(b, factor);
        var ci = Convert(c, factor);
        var di = Convert(d, factor);

        return IntersectProp(ai, bi, ci, di);
    }


    static public bool IntersectProp(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
    {
        // Eliminate improper cases. 
        if (
        Collinear(a, b, c) ||
        Collinear(a, b, d) ||
        Collinear(c, d, a) ||
        Collinear(c, d, b))
            return false;

        return
             Xor(Left(a, b, c), Left(a, b, d))
          && Xor(Left(c, d, a), Left(c, d, b));
    }

    // ---------------------------------------------------------------------
    // Exclusive or: true iff exactly one argument is true.
    //
    static protected bool Xor(bool x, bool y)
    {
        // The arguments are negated to ensure that they are 0/1 values. 
        // (Idea due to Michael Baldwin.) 
        return !x ^ !y;
    }




    public enum LineSegmentIntersectionType
    {
        IntersectionCollinear, // e - forms a subset line segment (or maybe a vertex)
        IntersectionEndPointNotCollinear, // v - just a vertex but shared with line segment
        IntersectionProper, // 1
        NonIntersecting // 0
    }


    // Like the SegSegInt() below but a, b, c, d, p, q are all mapped from floating point to integers and back again
    static public LineSegmentIntersectionType SegSegInt(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 p, out Vector2 q)
    {
        return SegSegInt(a, b, c, d, out p, out q, FloatToIntFactor);
    }

    // Like the SegSegInt() below but a, b, c, d, p, q are all mapped from floating point to integers and back again
    static public LineSegmentIntersectionType SegSegInt(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 p, out Vector2 q, int factor)
    {
        var f = factor;
        Vector2Double pd = new Vector2Double();
        Vector2Double qd = new Vector2Double();

        Vector2Int pi = new Vector2Int();
        Vector2Int qi = new Vector2Int();

        var ret = SegSegInt(Convert(a, f), Convert(b, f), Convert(c, f), Convert(d, f), out pd, out qd, out pi, out qi);

        p = new Vector2((float)(pd.x / (double)factor), (float)(pd.y / (double)factor));
        q = new Vector2((float)(qd.x / (double)factor), (float)(qd.y / (double)factor));

        return ret;
    }


    //---------------------------------------------------------------------
    // SegSegInt: Finds the point of intersection p between two closed
    // segments ab and cd.  Returns p and a char with the following meaning:
    // 'e': The segments collinearly overlap, sharing [at least a] point.
    // 'v': An endpoint (vertex) of one segment is on the other segment,
    // but 'e' doesn't hold.
    // '1': The segments intersect properly (i.e., they share a point and
    // neither 'v' nor 'e' holds).
    // '0': The segments do not intersect (i.e., they share no points).
    // Note that two collinear segments that share just one point, an endpoint
    // of each, returns 'e' rather than 'v' as one might expect.
    //---------------------------------------------------------------------

    // p and q define the overlap line segment end points if collinear intersection occurs
    // If and q are equal then collinear intersection occurs at only a point
    // Any non collinear intersection, only p is set to the intersection point

    // Note that even though p an q are vector2 doubles, they are NOT mapped to
    // the reals/floats with Convert() declared above. Instead, they provide sub-cell resolution
    // in discretized space coords
    // pi and qi and used to return true integer solutions for IntersectionCollinear and IntersectionEndPointNotCollinear
    // otherwise, they are rounded for other intersection types. System.Math.Round()
    // is used for rounding with nearest integer and nearest even int for midpoint values

    static public LineSegmentIntersectionType SegSegInt(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d, out Vector2Double p, out Vector2Double q,
        out Vector2Int pi, out Vector2Int qi)
    {
        double s, t;       // The two parameters of the parametric eqns. 
        double num, denom;  // Numerator and denoninator of equations. 
        //char code = '?';    // Return char characterizing intersection. 
        LineSegmentIntersectionType code = LineSegmentIntersectionType.NonIntersecting;

        p = new Vector2Double();
        q = new Vector2Double();
        pi = new Vector2Int();
        qi = new Vector2Int();

        //p.x = p.y = 100.0;  // For testing purposes only... 

        denom = a.x * (double)(d.y - c.y) +
                b.x * (double)(c.y - d.y) +
                d.x * (double)(b.y - a.y) +
                c.x * (double)(a.y - b.y);

        // If denom is zero, then segments are parallel: handle separately. 
        if (denom == 0.0)
        {

            var ret = ParallelInt(a, b, c, d, out pi, out qi);

            p = new Vector2Double(pi.x, pi.y);
            q = new Vector2Double(qi.x, qi.y);

            return ret;
        }

        num = a.x * (double)(d.y - c.y) +
             c.x * (double)(a.y - d.y) +
                 d.x * (double)(c.y - a.y);


        if(num == 0.0)
        {
            code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; // 'v';
            pi = a;
            //return code;
        }

        if(num == denom)
        {
            code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; // 'v';
            pi = b;
            //return code;
        }

        //if ((num == 0.0) || (num == denom)) code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; // 'v';

        s = num / denom;

        //Debug.Log("SegSegInt: num=" + num + ",denom=" + denom + ",s=" + s);

        num = -(a.x * (double)(c.y - b.y) +
             b.x * (double)(a.y - c.y) +
             c.x * (double)(b.y - a.y));

        if (num == 0.0)
        {
            code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; // 'v';
            pi = c;
            p = new Vector2Double(pi.x, pi.y);
            //return code;
        }

        if (num == denom)
        {
            code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; // 'v';
            pi = d;
            p = new Vector2Double(pi.x, pi.y);
            //return code;
        }

        //if ((num == 0.0) || (num == denom)) code = LineSegmentIntersectionType.IntersectionEndPointNotCollinear; //'v';

        t = num / denom;

        //Debug.Log("SegSegInt: num=" + num + ",denom=" + denom + ",t=" + t);

        if ((0.0 < s) && (s < 1.0) &&
              (0.0 < t) && (t < 1.0))
            code = LineSegmentIntersectionType.IntersectionProper; //'1';
        else if ((0.0 > s) || (s > 1.0) ||
              (0.0 > t) || (t > 1.0))
            code = LineSegmentIntersectionType.NonIntersecting; //'0';


        if (code != LineSegmentIntersectionType.IntersectionEndPointNotCollinear)
        {
            p.x = a.x + s * (b.x - a.x);
            p.y = a.y + s * (b.y - a.y);

            pi.x = (int)System.Math.Round(p.x);
            pi.y = (int)System.Math.Round(p.y);
        }

        return code;
    }



    static public int AreaSign(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        double area2;

        area2 = (b.x - a.x) * (double)(c.y - a.y) -
                (c.x - a.x) * (double)(b.y - a.y);


        // The area should be an integer. 
        if (area2 > 0.5) return 1;
        else if (area2 < -0.5) return -1;
        else return 0;
    }


    //---------------------------------------------------------------------
    //Returns true iff c is strictly to the left of the directed
    //line through a to b.
    //
    static public bool Left(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return AreaSign(a, b, c) > 0;
    }

    static public bool LeftOn(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return AreaSign(a, b, c) >= 0;
    }

    static public bool Collinear(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return AreaSign(a, b, c) == 0;
    }



    static public bool Collinear(Vector2Int[] poly)
    {
        if (poly == null)
            return false;

        var polylen = poly.Length;

        if (polylen < 3)
            return true;

        for(int i = 0; i < polylen - 2; ++i)
        {
            var p1 = poly[i];
            var p2 = poly[i + 1];
            var p3 = poly[i + 2];

            if (!Collinear(p1, p2, p3))
                return false;
        }

        return true;

    }


    //static public void Assigndi(cPointd p, cPointi a)
    //{
    //    p.x = a.x;
    //    p.y = a.y;
    //}



    //---------------------------------------------------------------------
    //Returns true iff point c lies on the closed segement ab.
    //First checks that c is collinear with a and b.
    //
    static public bool Between(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        //Vector2Int ba, ca;

        if (!Collinear(a, b, c))
            return false;

        // If ab not vertical, check betweenness on x; else on y. 
        if (a.x != b.x)
            return ((a.x <= c.x) && (c.x <= b.x)) ||
               ((a.x >= c.x) && (c.x >= b.x));
        else
            return ((a.y <= c.y) && (c.y <= b.y)) ||
               ((a.y >= c.y) && (c.y >= b.y));
    }


    //---------------------------------------------------------------------
    // Returns TRUE iff point c lies on the closed segement ab.
    // Assumes it is already known that abc are collinear.
    // (This is the only difference with Between().)
    //---------------------------------------------------------------------
    static public bool Between1(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        //Vector2Int ba, ca;

        /* If ab not vertical, check betweenness on x; else on y. */
        if (a.x != b.x)
            return ((a.x <= c.x) && (c.x <= b.x)) ||
          ((a.x >= c.x) && (c.x >= b.x));
        else
            return ((a.y <= c.y) && (c.y <= b.y)) ||
          ((a.y >= c.y) && (c.y >= b.y));
    }



    // multi poly version of below
    static public bool IsLineSegmentCoincidentWithEdgeInPolys(Vector2Int A, Vector2Int B, List<Polygon> polys)
    {

        foreach(var poly in polys)
        {
            if (IsLineSegmentCoincidentWithLineInPoly(A, B, poly))
                return true;
        }

        return false;

    }

    // checks for collinear and overlapping more than just a point (True). False otherwise
    static public bool IsLineSegmentCoincidentWithLineInPoly(Vector2Int A, Vector2Int B, Polygon poly)
    {
        var pts = poly.getIntegerPoints();

        for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
        {
            Vector2Int x;
            Vector2Int y;

            var res = ParallelInt(A, B, pts[j], pts[i], out x, out y);

            // collinear with more than just endpoints overlapping
            if (res == LineSegmentIntersectionType.IntersectionCollinear && (x != y))
            {
                //Debug.Log($"Coincident because {x} != {y} overlap of {A}, {B} and {pts[j]}, {pts[i]}");

                return true;
            }
        }

        return false;
    }




    static public LineSegmentIntersectionType ParallelInt(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d, out Vector2Int p, out Vector2Int q)
    {

        if (!Collinear(a, b, c) || !Collinear(a, b, d))
        {
            p = new Vector2Int();
            q = new Vector2Int();
            return LineSegmentIntersectionType.NonIntersecting; //'0';
        }

        if (Between1(a, b, c) && Between1(a, b, d))
        {
            p = c;
            q = d;
            return LineSegmentIntersectionType.IntersectionCollinear; // 'e';
        }
        if (Between1(c, d, a) && Between1(c, d, b))
        {
            p = a;
            q = b;
            return LineSegmentIntersectionType.IntersectionCollinear; //'e';
        }
        if (Between1(a, b, c) && Between1(c, d, b))
        { 
            p = c;
            q = b;
            return LineSegmentIntersectionType.IntersectionCollinear; //'e';
        }
        if (Between1(a, b, c) && Between1(c, d, a))
        {
            p = c;
            q = a;
            return LineSegmentIntersectionType.IntersectionCollinear; //'e';
        }
        if (Between1(a, b, d) && Between1(c, d, b))
        {
            p = d;
            q = b;
            return LineSegmentIntersectionType.IntersectionCollinear; //'e';
        }
        if (Between1(a, b, d) && Between1(c, d, a))
        {
            p = d;
            q = a;
            return LineSegmentIntersectionType.IntersectionCollinear; //'e';
        }

        p = new Vector2Int();
        q = new Vector2Int();

        return LineSegmentIntersectionType.NonIntersecting; //'0';


    }


    public enum PointPolygonIntersectionType
    {
        Vertex,
        Edge,
        Inside,
        Outside
    }

    static public PointPolygonIntersectionType InPoly1(Vector2[] poly, Vector2 q)
    {
        return InPoly1(poly, q, FloatToIntFactor);
    }

    static public PointPolygonIntersectionType InPoly1(Vector2[] poly, Vector2 q, int factor)
    {
        var qi = Convert(q, factor);

        Vector2Int[] polyi = Convert(poly, factor);

        return InPoly1(polyi, qi);
    }



    // Returns the strongest PointPolygonIntersectionType found
    // strongest to weakest: Inside, Edge, Vertex, Outside
    // The first Inside found immediately returns. Otherwise, all polys analyzed

    static public PointPolygonIntersectionType InPoly1(List<Polygon> polys, Vector2Int q)
    {
        var res = PointPolygonIntersectionType.Outside;

        foreach(var poly in polys)
        {
            var temp = InPoly1(poly.getIntegerPoints(), q);

            if (temp == PointPolygonIntersectionType.Inside)
                return PointPolygonIntersectionType.Inside;

            if(res == PointPolygonIntersectionType.Outside)         
                res = temp;
            else if(res == PointPolygonIntersectionType.Vertex)
            {
                if (temp == PointPolygonIntersectionType.Edge)
                    res = PointPolygonIntersectionType.Edge;
            }
                    
        }

        return res;
    }


    // Test if point on/inside poly

    // See Chapter 7 for an explanation of this code
    // The only modification we make is that the polygon is *not* translated 
    // to place q=(0,0)

    static public PointPolygonIntersectionType InPoly1(Vector2Int[] poly, Vector2Int q)
    {

        double x;
        int Rcross = 0;
        int Lcross = 0;
        bool Rstrad, Lstrad;

        //List<int> inters = new List<int>();

        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {

            Vector2Int vtemp = poly[j];
            Vector2Int vtemp1 = poly[i];


            // First check if q =(0,0) is a vertex.
            if (vtemp.x == q.x && vtemp.y == q.y)
                return PointPolygonIntersectionType.Vertex;

            // Check if e straddles x-axis, with bias above/below.
            Rstrad = ((vtemp.y - q.y) > 0) != ((vtemp1.y - q.y) > 0);
            Lstrad = ((vtemp.y - q.y) < 0) != ((vtemp1.y - q.y) < 0);

            if (Rstrad || Lstrad)
            {
                // Compute intersection of e with x-axis.
                x = ((vtemp.x - q.x) * (vtemp1.y - q.y) -
                     (vtemp1.x - q.x) * (vtemp.y - q.y))
                      / (double)((vtemp1.y - q.y) - (vtemp.y - q.y));
                // saving the x-coordinates for the intersections;
                //inters.Add((int)x + q.x);

                if (Rstrad && x > 0) Rcross++;
                if (Lstrad && x < 0) Lcross++;
            }

        }

        // q on an edge if L/Rcross counts are not the same parity
        if ((Rcross % 2) != (Lcross % 2)) return PointPolygonIntersectionType.Edge;
        // q inside iff an odd number of crossings
        if ((Rcross % 2) == 1) return PointPolygonIntersectionType.Inside;
        else
            return PointPolygonIntersectionType.Outside;


    }



    public static bool IsLineSegmentInPolygon(Vector2Int A, Vector2Int B, Vector2Int[] poly)
    {
        for(int i=0, j= poly.Length - 1; i < poly.Length; j=i++)
        {
            if ((A.Equals(poly[j]) && B.Equals(poly[i])) ||
                (B.Equals(poly[j]) && A.Equals(poly[i])))
                return true;
        }

        return false;
    }

    public static bool IsLineSegmentInPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
    {
        foreach (var poly in polys)
        {
            if (IsLineSegmentInPolygon(A, B, poly.getIntegerPoints()))
                return true;
        }

        return false;
    }


    //public static bool ParallelIntersectionOfLineSegmentWithEdgesOfPolygon(Vector2Int A, Vector2Int B, Vector2Int[] poly)
    //{
    //    Vector2Double p, q;


    //    for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
    //    {
    //        if (ParallelInt(A, B, poly[j], poly[i], out p, out q) == LineSegmentIntersectionType.IntersectionCollinear )
    //            return true;
    //    }

    //    return false;

    //}

    //public static bool ParallelIntersectionOfLineSegmentWithEdgesOfPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
    //{
    //    foreach (var poly in polys)
    //    {
    //        if (ParallelIntersectionOfLineSegmentWithEdgesOfPolygon(A, B, poly.getIntegerPoints()))
    //            return true;
    //    }

    //    return false;
    //}


    // The signed area of the triangle det. by a,b,c; pos. if ccw, neg. if cw
    static public int Area2(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        int area = ((c.x - b.x) * (a.y - b.y)) - ((a.x - b.x) * (c.y - b.y));
        return area;
    }



    // Returns area of a polygon formed by the list of vertices
    //

    static public int AreaPoly2(Vector2Int[] poly)
    {

        if (poly.Length < 3)
            return 0;

        int sum = 0;

        Vector2Int p = poly[0];

        for(int i = 1, k=2; i < poly.Length-1; ++i, ++k)
        {
            sum += Area2(p, poly[i], poly[k]);

        }

        return sum;

    }

    // Determine if the polygon/list is oriented counterclockwise (ccw).
     // TODO (A more efficient method is possible, but here we use the available
     // AreaPoly2())
     //
    static public bool Ccw(Vector2Int[] poly)
    {
        int sign = AreaPoly2(poly);
        if (sign > 0) return true;
        else return false;
    }


    static public bool Ccw(Vector2[] poly)
    {
        Vector2Int[] ipoly = Convert(poly);

        return Ccw(ipoly);
    }



    // Returns true if polygon is covex, else returns false  
    static public bool CheckForConvexity(Vector2Int[] poly)
    {

        for (int i = 0, j = poly.Length - 1, k = poly.Length - 2; i < poly.Length; k = j, j = i++)
        {
            if (!LeftOn(poly[k], poly[j], poly[i]))
                return false;
        }

        return true;

    }


    public static List<Vector2Int> PolysIntersections(List<Polygon> polys)
    {
        var intersections = new List<Vector2Int>();

        if (polys == null)
            return intersections;

        var polysCount = polys.Count;

        for(int i = 0; i < polysCount; ++i)
        {
            var polyA = polys[i];

            for(int j=i+1; j < polysCount; ++j)
            {
                var polyB = polys[j];

                var ret = PolyPolyEdgeIntersections(polyA.getIntegerPoints(), polyB.getIntegerPoints());
                intersections.AddRange(ret);
            }

        }

        return intersections;

    }

    public static List<Vector2Int> PolyPolyEdgeIntersections(Vector2Int[] polyA, Vector2Int[] polyB)
    {

        var intersections = new List<Vector2Int>();

        var polyALen = polyA.Length;
        var ccwA = Ccw(polyA);

        if (polyA == null || polyALen < 3)
        {
            Debug.LogError($"Bad poly passed: vert count: {polyALen} ccw?: {ccwA}");
            return intersections;
        }



        var polyBLen = polyB.Length;
        var ccwB = Ccw(polyB);

        if (polyB == null || polyBLen < 3)
        {
            Debug.LogError($"Bad poly passed: vert count: {polyBLen} ccw?: {ccwB}");
            return intersections;
        }



        for(int i = 0, j = polyALen - 1; i < polyALen; j = i++)
        {

            var A = polyA[j];
            var B = polyA[i];

            for(int k = 0, l = polyBLen - 1; k < polyBLen; l = k++ )
            {
                var C = polyB[l];
                var D = polyB[k];

                var ret = SegSegInt(A, B, C, D, out var p, out var q, out var pi, out var qi);

                if(ret == LineSegmentIntersectionType.IntersectionProper)
                {
                    intersections.Add(pi);
                }

            }

        }

        return intersections;

    }



    // Tests whether two polygons intersect (touch or overlap), returning true if so, false otherwise
    public static bool PolyPolyIntersects(Vector2Int[] polyA, Vector2Int[] polyB)
    {

        var polyALen = polyA.Length;
        var ccwA = Ccw(polyA);

        if (polyA == null || polyALen < 3)
        {
            Debug.LogError($"Bad poly passed: vert count: {polyALen} ccw?: {ccwA}");
            return false;
        }



        var polyBLen = polyB.Length;
        var ccwB = Ccw(polyB);

        if (polyB == null || polyBLen < 3)
        {
            Debug.LogError($"Bad poly passed: vert count: {polyBLen} ccw?: {ccwB}");
            return false;
        }


        // edge intersections (including touching) handle all overlaps including
        // through vertices, and perfectly aligned identical polygons

        for (int i = 0, j = polyALen - 1; i < polyALen; j = i++)
        {
            var A = polyA[j];
            var B = polyA[i];

            if (IntersectionLineSegmentWithPolygon(A, B, polyB))
            {
                return true;
            }
        }

        // check for points of one poly inside other. Will indicate a fully contained
        // polygon

        for (int i = 0; i < polyALen; i++)
        {
            if (InPoly1(polyB, polyA[i]) != PointPolygonIntersectionType.Outside)
                return true;
        }


        for (int i = 0; i < polyBLen; i++)
        {
            if (InPoly1(polyA, polyB[i]) != PointPolygonIntersectionType.Outside)
                return true;
        }


        return false;
    }


    public static bool IntersectionLineSegmentWithPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
    {
        foreach (var poly in polys)
        {
            if (IntersectionLineSegmentWithPolygon(A, B, poly.getIntegerPoints()))
                return true;
        }

        return false;
    }


    // (OutsideOrOnToOnOrInside) IntersectionLineSegmentWithPolygon():
    // This tests whether a line segment that starts outside (or on) a polygon crosses inside
    // (or stays on or touches the outside). It doesn't test for segment AB entirely inside the poly.
    // In short, this method returns true if a line segment touches one or more of the poly segments
    public static bool IntersectionLineSegmentWithPolygon(Vector2Int A, Vector2Int B, Vector2Int[] poly)
    {


        var len = poly.Length;
        var ccw = Ccw(poly);

        if (poly == null || len < 3 || !ccw)
        {
            Debug.LogError($"Bad poly passed: vert count: {len} ccw?: {ccw}");
            return false;
        }

        for (int i = 0, j = len - 1; i < len; j = i++)
        {
            var C = poly[j];
            var D = poly[i];

            if(Intersect(A, B, C, D))
            {
                return true;
            }
        }

        return false;
    }


    public static bool InteriorIntersectionLineSegmentWithPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
    {
        foreach(var poly in polys)
        {
            if (InteriorIntersectionLineSegmentWithPolygon(A, B, poly.getIntegerPoints()))
                return true;
        }

        return false;
    }



    //static bool debugHelper(Vector2Int A, Vector2Int B)
    //{
    //    Vector2Int m = new Vector2Int(-6900, 3100);
    //    Vector2Int n = new Vector2Int(-6900, 2650);

    //    return (A == m && B == n) || (A == n && B == m);
    //}




    // This method finds whether intersection of line segments with polygons where the segment begins either outside or on the polygon
    // and crosses into the interior occurs. Any line segment that stays outside or on the polygon will not be counted
    // as intersecting. Any line segment that is completely inside the polygon will also not count as intersecting.
    // Segments that cross in one side and out the other (possibly multiple times for concave polys) will count as intersecting.

    // Note that the polygon to test against must be convex or concave and closed with CCW winding.
    // Degenerate polys (zero len edges, self intersection, hair, holes) could all cause problems. Degenerate cases are not
    // tested for currently and should be considered undefined behavior.
    //
    //
    // Based on Joseph O'Rourke's solution discussion here: https://stackoverflow.com/questions/3742382/segment-polygon-intersection

    // Although his discussion appears to be incomplete. This solution was revised to consider convex and concave vertex intersections
    // separately.


    public static bool InteriorIntersectionLineSegmentWithPolygon(Vector2Int A, Vector2Int B, Vector2Int[] poly)
    {
        Vector2Double p, q;
        Vector2Int pi, qi;

        var len = poly.Length;
        var ccw = Ccw(poly);

        if (poly == null || len < 3 || !ccw)
        {
            Debug.LogError($"Bad poly passed: vert count: {len} CCW?: {ccw}");
            return false;
        }

        for (int i = 0, j = len - 1, k = len - 2, l = len - 3; i < len; l = k, k = j, j = i++)
        {
            var C = poly[k];
            var D = poly[j];

            var res = SegSegInt(A, B, C, D, out p, out q, out pi, out qi);


            if (res == LineSegmentIntersectionType.IntersectionProper)
            {
                //if(debugHelper(A,B)) //TODO del me
                //    Debug.Log("Proper intersection found!");
                return true;
            }

            if(res == LineSegmentIntersectionType.IntersectionEndPointNotCollinear)
            {
                var curr = pi;
                Vector2Int prev;
                Vector2Int next;

                if(curr.Equals(C))
                {
                    prev = poly[l];
                    next = poly[j];
                }
                else if(curr.Equals(D))
                {
                    prev = poly[k];
                    next = poly[i];
                }
                else
                {
                    // this shouldn't happen with navmesh verts
                    // but in general a seg could end on a poly seg (not endpt)
                    // One should still be able to use Left() with an inserted
                    // poly point though


                    prev = poly[k];
                    next = poly[j];
                }


                // This logic relies on the fact that we have excluded collinear intersections
                // in parent if clause. In cases of collinear intersection, the next poly
                // segment in sequence cannot also be collinear unless degenerate (e.g. hair).
                //

                if(LeftOn(prev, curr, next))
                {
                    // convex angle on poly
                    if (
                        (Left(prev, curr, A) && Left(curr, next, A)) ||
                        (Left(prev, curr, B) && Left(curr, next, B))
                        )
                    {
                        //if (debugHelper(A, B)) //TODO del me
                        //{
                        //    Debug.Log($"convex intersection with: {prev}, {curr}, {next}");
                        //    Debug.Log($"convex int C and D: {C}, {D}");
                        //    Debug.Log($"convex int A and B: {A}, {B}");
                        //}

                        return true;
                    }
                }
                else
                {
                    // concave angle on poly
                    if (
                        ( !(!Left(prev, curr, A) && !Left(curr, next, A))) ||
                        ( !(!Left(prev, curr, B) && !Left(curr, next, B)))
                        )
                    {
                        return true;
                    }
                }
                
            }

        }

        return false;

    }


    static bool ConvIntersHelper(Vector2Int[] A, Vector2Int[] B)
    {

        // find a line seg of A that keeps the rest of A on the left and all of
        // B on the right (or touching)
        for(int i = 0, j = A.Length - 1; i < A.Length; j= i++)
        {

            bool foundDivLine = true;

            for(int k = 0; k < B.Length; ++k)
            {
                if(Left(A[j], A[i], B[k]))
                {
                    foundDivLine = false;
                    break;
                }
            }

            if (foundDivLine)
                return false;


        }

        // no div line found so must intersect
        return true;
    }


    // This solution uses the dividing line test with O'Rourke's Left()
    public static bool IntersectionConvexPolygons(Vector2Int[] A, Vector2Int[] B)
    {

        if (A == null || B == null || A.Length < 3 || B.Length < 3)
            return false;

        if (!CheckForConvexity(A) || !CheckForConvexity(B))
        {
            Debug.LogError("Attempt to find convex intersection with non-convex poly(s)!");
            return false;
        }

        // look for dividing lines
        if (!ConvIntersHelper(A, B))
            return false;

        if (!ConvIntersHelper(B, A))
            return false;

        // must intersect
        return true;

    }


    public static bool IntersectionConvexPolygons(Polygon A , Polygon B)
    {
        return IntersectionConvexPolygons(A.getIntegerPoints(), B.getIntegerPoints());
    }


    public static bool IntersectionConvexPolygons(Polygon A, List<Polygon> polys)
    {

        foreach (var poly in polys)
        {
            if (IntersectionConvexPolygons(A, poly))
                return true;
        }
        return false;
    }



    // Finds polygon's center of gravity 
    static public void FindCG(Vector2Int[] poly, out Vector2Double centerOfG)
    {

        centerOfG = new Vector2Double(0.0, 0.0);

        //  error check poly

        if (poly == null || poly.Length < 3)
            return;

        double A2, areaSum2 = 0;    //partial area sum
        Vector2Int Cent3 = new Vector2Int();
        //cVertex temp = list.head;
        Vector2Int orourke_fixed = poly[poly.Length - 1];


        //do
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++ )
        {
            Vector2Int temp = poly[j];
            Vector2Int tempNext = poly[i];

            Cent3 = Centroid3(orourke_fixed, temp, tempNext);
            A2 = Area2(orourke_fixed, temp, tempNext);
            centerOfG =  new Vector2Double(centerOfG.x + A2 * Cent3.x, centerOfG.y + A2 * Cent3.y);
   
            areaSum2 = areaSum2 + A2;

            //temp = temp.next;
        }
        //while (temp != list.head.prev);


        //Division by 3 is delayed to the last moment.

        var denom = (3.0 * areaSum2);
        var b = centerOfG.x;
        centerOfG = new Vector2Double(centerOfG.x / denom, centerOfG.y / denom);

    }



    // Centroid of triangle is just an average of vertices
 
    static public Vector2Int Centroid3(Vector2Int p1, Vector2Int p2, Vector2Int p3)
    {
        Vector2Int c = new Vector2Int();
        c.x = p1.x + p2.x + p3.x;
        c.y = p1.y + p2.y + p3.y;
        return c;
    }


    ////The signed area of the triangle det. by a,b,c; pos. if ccw, neg. if cw

    //static public int Area2(Vector2Int a, Vector2Int b, Vector2Int c)
    //{
    //    int area = ((c.x - b.x) * (a.y - b.y)) - ((a.x - b.x) * (c.y - b.y));
    //    return area;
    //}





    public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //Some nice optimizations from Grumdrig https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment

        // Return minimum distance between line segment vw and point p
        float l2 = (lineEnd - lineStart).sqrMagnitude;  // i.e. |w-v|^2 -  avoid a sqrt
        var lineStartToPoint = point - lineStart;
        if (l2 == 0.0) return lineStartToPoint.magnitude;   // v == w case
                                                            // Consider the line extending the segment, parameterized as v + t (w - v).
                                                            // We find projection of point p onto the line. 
                                                            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                            // We clamp t from [0,1] to handle points outside the segment vw.
        var lineStartToLineEnd = lineEnd - lineStart;
        float t = Mathf.Max(0f, Mathf.Min(1f, Vector2.Dot(lineStartToPoint, lineStartToLineEnd) / l2));
        Vector2 projection = lineStart + t * (lineStartToLineEnd);  // Projection falls on the segment

        return Vector2.Distance(point, projection);
    }



    public static int Dot(Vector2Int a, Vector2Int b)
    {
        return a.x * b.x + a.y * b.y;
    }

    public static float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd)
    {
        //Some nice optimizations from Grumdrig https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        
        // Return minimum distance between line segment vw and point p
        var l2 = (lineEnd - lineStart).sqrMagnitude;  // i.e. |w-v|^2 -  avoid a sqrt
        var lineStartToPoint = point - lineStart;
        if (l2 == 0) return lineStartToPoint.magnitude;   // v == w case
                                                          // Consider the line extending the segment, parameterized as v + t (w - v).
                                                          // We find projection of point p onto the line. 
                                                          // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                          // We clamp t from [0,1] to handle points outside the segment vw.
        var lineStartToLineEnd = new Vector2(lineEnd.x - lineStart.x, lineEnd.y - lineStart.y);
        //var lineStartToLineEnd = lineEnd - lineStart;
        float t = Mathf.Max(0f, Mathf.Min(1f, Vector2.Dot(lineStartToPoint, lineStartToLineEnd) / (float)l2));
        Vector2 projection = lineStart + t * (lineStartToLineEnd);  // Projection falls on the segment

        var dist = Vector2.Distance(point, projection);

        //Debug.Log($"Distance: {dist}");

        return dist;
    }


}
