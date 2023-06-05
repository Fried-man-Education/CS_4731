using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNetworkCase
{

    public int caseCount { get; private set; }

    public Vector2 canvasOrigin { get; private set; }
    public float canvasWidth { get; private set; }
    public float canvasHeight { get; private set; }
    //public float cellSize { get; private set; }
    public List<Polygon> obstacles { get; private set; }
    public Vector2[][] obstaclePoints { get; private set; }
    public float agentRadius { get; private set; }
    //public GridConnectivity gridConnectivity { get; private set; }
    //public bool[,] grid { get; private set; }
    public List<Vector2> pathNodes { get; private set; }
    public Vector2[] pathNodePoints { get; private set; }
    public List<List<int>> pathEdges { get; private set; }
    public int[][] pathEdgePoints { get; private set; }


    // for creating to check for match
    public PathNetworkCase(int _caseCount, Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
        List<Polygon> obstacles, float agentRadius, List<Vector2> _pathNodes)
    {
        this.caseCount = _caseCount;

        this.canvasOrigin = canvasOrigin;
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        //this.cellSize = cellSize;
        this.obstacles = obstacles;
        this.agentRadius = agentRadius;
        //this.gridConnectivity = gridConnectivity;

        Vector2[][] obstaclePoints = new Vector2[obstacles.Count][];

        for (int i = 0; i < obstaclePoints.Length; ++i)
        {
            var pts = obstacles[i].getPoints();
            obstaclePoints[i] = new Vector2[pts.Length];

            for (int j = 0; j < pts.Length; ++j)
            {
                obstaclePoints[i][j] = pts[j];
            }
        }

        this.obstaclePoints = obstaclePoints;

        this.pathNodes = _pathNodes;

        this.pathNodePoints = this.pathNodes.ToArray();


    }


    // for creating for storing
    public PathNetworkCase(
        int _caseCount,
        Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
        List<Polygon> obstacles,
        float agentRadius,
        List<Vector2> _pathNodes, List<List<int>> _pathEdges
) : this(_caseCount, canvasOrigin, canvasWidth, canvasHeight, obstacles, agentRadius, _pathNodes)
    {


        this.pathEdges = _pathEdges;

        this.pathEdgePoints = new int[this.pathEdges.Count][];

        for (int i = 0; i < this.pathEdgePoints.Length; ++i)
        {
            var pts = this.pathEdges[i];
            this.pathEdgePoints[i] = new int[pts.Count];
            for (int j = 0; j < pts.Count; ++j)
            {
                this.pathEdgePoints[i][j] = pts[j];
            }
        }

    }



    // for creating from stored record
    public PathNetworkCase(
        int _caseCount,
        Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
        Vector2[][] obstaclePoints,
        float agentRadius,
        Vector2[] pathNodePts, int[][] pathEdgePts
)
    {
        this.caseCount = _caseCount;

        this.canvasOrigin = canvasOrigin;
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        //this.cellSize = cellSize;
        this.agentRadius = agentRadius;
        //this.gridConnectivity = gridConnectivity;
        this.obstaclePoints = obstaclePoints;

        List<Polygon> obstacles = new List<Polygon>();

        for (int i = 0; i < obstaclePoints.Length; ++i)
        {
            var ob = new Polygon();
            ob.SetPoints(obstaclePoints[i]);
            obstacles.Add(ob);
        }

        this.obstacles = obstacles;
        //this.agentRadius = agentRadius;

        this.pathNodePoints = pathNodePts;

        List<Vector2> pNodes = new List<Vector2>(pathNodePoints);

        this.pathNodes = pNodes;

        this.pathEdgePoints = pathEdgePts;

        List<List<int>> pEdges = new List<List<int>>();

        for (int i = 0; i < pathEdgePts.Length; ++i)
        {
            List<int> edges = new List<int>(pathEdgePts[i]);
            pEdges.Add(edges);
        }
        this.pathEdges = pEdges;

    }



    //bool pathNodePointsEqual(Vector2[] o)
    //{
    //    var oo = this.pathNodePoints;
    //    if (oo == null && o == null)
    //        return true;

    //    if ((oo != null && o == null) || (oo == null && o != null))
    //        return false;

    //    if (oo.Length != o.Length)
    //        return false;

    //    for (int i = 0; i < oo.Length; ++i)
    //    {
    //        if (o[i] != oo[i])
    //            return false;
    //    }

    //    return true;
    //}


    bool approxEquals(Vector2 a, Vector2 b)
    {
        var epsilon = 0.001f;

        //Debug.Log($"{a.x} - {b.x} == {a.x - b.x} Equals? {Mathf.Abs(a.x - b.x) <= epsilon}");
        //Debug.Log($"{a.y} - {b.y} == {a.y - b.y} Equals? {Mathf.Abs(a.y - b.y) <= epsilon}");

        if (Mathf.Abs(a.x - b.x) > epsilon)
            return false;

        if (Mathf.Abs(a.y - b.y) > epsilon)
            return false;

        return true;
    }

    bool pathNodePointsEqual(Vector2[] o)
    {
        var oo = this.pathNodePoints;
        if (oo == null && o == null)
            return true;

        if ((oo != null && o == null) || (oo == null && o != null))
            return false;

        if (oo.Length != o.Length)
            return false;

        for (int i = 0; i < oo.Length; ++i)
        {
            //if (o[i] != oo[i])
            if(!approxEquals(o[i], oo[i]))
                return false;
        }

        return true;
    }


    bool obstaclesEqual(Vector2[][] o)
    {
        var oo = this.obstaclePoints;
        if (oo == null && o == null)
            return true;

        if ((oo != null && o == null) || (oo == null && o != null))
            return false;

        if (oo.Length != o.Length)
            return false;

        for (int i = 0; i < oo.Length; ++i)
        {
            if (oo[i] == null && o[i] == null)
                continue;

            if ((oo[i] != null && o[i] == null) || (oo[i] == null && o[i] != null))
                return false;

            if (oo[i].Length != o[i].Length)
                return false;

            for (int j = 0; j < oo[i].Length; ++j)
            {
                //if (oo[i][j] != o[i][j])
                if (!approxEquals(oo[i][j], o[i][j]))
                    return false;
            }
        }

        return true;
    }

    public bool Equals(PathNetworkCase p)
    {
        //Debug.Log("BEGIN");
        //if (this.canvasOrigin != p.canvasOrigin) Debug.Log("origin");
        //if (this.canvasWidth != p.canvasWidth) Debug.Log("Width");
        //if (this.canvasHeight != p.canvasHeight) Debug.Log($"Height: {this.canvasHeight} != {p.canvasHeight}");     
        //if (this.agentRadius != p.agentRadius) Debug.Log("agent Radius");
        //if (!obstaclesEqual(p.obstaclePoints)) Debug.Log("obstacles");
        //if (!pathNodePointsEqual(p.pathNodePoints)) Debug.Log("path nodes");

        //we don't compare generated stuff (grid, pathnodes/edges) because that is what we are trying to find
        if (this.canvasOrigin != p.canvasOrigin ||
        this.canvasWidth != p.canvasWidth ||
        this.canvasHeight != p.canvasHeight ||
        this.agentRadius != p.agentRadius ||
        //this.gridConnectivity != p.gridConnectivity ||
        //this.cellSize != p.cellSize ||
        //!this.pathNodePoints.SequenceEqual(p.pathNodePoints) ||
        !pathNodePointsEqual(p.pathNodePoints) ||
        !obstaclesEqual(p.obstaclePoints)
            )
            return false;

        //Debug.Log("EQUALS!");
        return true;
    }


    //    Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float cellSize,
    //Vector2[][] obstaclePoints, GridConnectivity gridConnectivity,
    //        char[,] gridMap, Vector2[] pathNodePts, int[][] pathEdgePts

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        var nl = System.Environment.NewLine;
        sb.Append($"static PathNetworkCase pn{caseCount} = new PathNetworkCase(" + nl);
        sb.Append($"{this.caseCount}, " + nl);
        sb.Append($"new Vector2({this.canvasOrigin.x}f, {this.canvasOrigin.y}f), " + nl);
        sb.Append($"{this.canvasWidth}f, {this.canvasHeight}f, " + nl);
        //sb.Append($"{this.cellSize}f, " + nl);

        sb.Append($"new Vector2[][] {{" + nl);
        foreach (var op in this.obstaclePoints)
        {
            sb.Append($"new Vector2[] {{");

            foreach (var v in op)
            {
                sb.Append($"new Vector2({v.x}f, {v.y}f), ");
            }
            //sb.Append(nl);
            sb.Append($"}}, " + nl);
        }
        sb.Append($"}}, " + nl);

        sb.Append($"{this.agentRadius}f, " + nl);

        //sb.Append($"GridConnectivity.{this.gridConnectivity}, " + nl);



        sb.Append($"new Vector2[] {{");
        foreach (var v in this.pathNodePoints)
        {
            sb.Append($"new Vector2({v.x}f, {v.y}f), ");
        }
        //sb.Append(nl);
        sb.Append($"}}, " + nl);

        sb.Append($"new int[][] {{");

        foreach (var pe in this.pathEdgePoints)
        {
            sb.Append($"new int[] {{");
            foreach (var p in pe)
            {
                sb.Append($"{p}, ");
            }
            //sb.Append(nl);
            sb.Append($"}}, " + nl);
        }

        sb.Append($"}});" + nl);

        return sb.ToString();
    }


}
