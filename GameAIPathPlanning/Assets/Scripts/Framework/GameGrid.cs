//#define SAVE_CASES

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using GameAICourse;

public class GameGrid : DiscretizedSpaceMonoBehavior
{

    public float CellSize = 0.2f;

    public GridConnectivity gridConnectivity = GridConnectivity.FourWay;

    public GridConnectivity GridConn
    {
        get
        {
            return gridConnectivity;
        }
    }

    public bool[,] Grid { get; protected set; }

    List<List<int>> hardCodedAdjacencies = null;

    public Color LineColor = Color.green;
    public Color BlockedLineColor = Color.blue;


    public bool VisualizePathNetwork = false;

    public override void Awake()
    {
        base.Awake();

        Obstacles = this.GetComponent<Obstacles>();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        // Following commented out because presets will immediately override...
        //Obstacles.Init();

        //Bake();

        if(UseHardCodedCases)
             Utils.DisplayName("CreateGrid", "HARD CODED CASES");
        else
            Utils.DisplayName("CreateGrid", CreateGrid.StudentAuthorName);

    }

#if SAVE_CASES
    int caseCount = 0;
#endif

    public bool UseHardCodedCases = false;

    bool hardCodedCaseFound = false;

    override public void Bake()
    {
        base.Bake();

        bool[,] grid;
        //List<Vector2> pathNodes = new List<Vector2>();
        //List<List<int>> pathEdges = new List<List<int>>();

        var obst = Obstacles.getObstacles();

        var polys = new List<Polygon>(obst.Count);

        for(int i=0; i < obst.Count; ++i)
        {
            polys.Add(obst[i].GetPolygon());
        }

        GridCase overrideCase = null;

        if (UseHardCodedCases)
        {
            hardCodedCaseFound = false;

            var gct = new GridCase(0, BottomLeftCornerWCS, Boundary.size.x, Boundary.size.z, CellSize, polys, gridConnectivity);

            overrideCase = HardCodedGridCases.FindCase(gct);
        }

        if (overrideCase != null)
        {
            hardCodedCaseFound = true;

            Debug.Log($"Grid hard-coded case {overrideCase.caseCount} is being used");
            grid = overrideCase.grid;
            hardCodedAdjacencies = overrideCase.adjacencies;

            if (hardCodedAdjacencies == null || hardCodedAdjacencies.Count == 0)
                Debug.Log("Hard-coded adjacencies null or empty");

            //pathNodes = overrideCase.pathNodes;
            //pathEdges = overrideCase.pathEdges;
        }
        else
        {

            if(UseHardCodedCases)
            {
                Debug.Log("WARNING! Couldn't find a hard-coded case. Falling back to generation.");
            }

            //pathNodes = new List<Vector2>();
            //pathEdges = new List<List<int>>();
            //grid = new bool[1, 1];
            //grid[0, 0] = true;

            //Debug.Log("Grid: calling student code!");

            CreateGrid.Create(BottomLeftCornerWCS, Boundary.size.x, Boundary.size.z, CellSize,
                                polys, out grid);


        }

        Grid = grid;
        //PathNodes = pathNodes;
        //PathEdges = pathEdges;


#if SAVE_CASES

        var adjs = SaveAllAdjacencies();

        GridCase gc = new GridCase(caseCount++, BottomLeftCornerWCS, Boundary.size.x, Boundary.size.z, CellSize, polys, GridConn, grid, adjs); // pathNodes, pathEdges);

        using (var OutFile = new StreamWriter("cases.txt", true))
        {

            OutFile.WriteLine(gc);
        }

#endif




        PurgeOutdatedLineViz();

        if (grid != null)
        {
            CreateGridLines(Grid, CellSize, BottomLeftCornerWCS);
        }

        if (VisualizePathNetwork)
        {
            if (PathNodes != null && PathEdges != null)
                CreateNetworkLines(PathOverlay_OffsetFromFarCP);
        }

        GridOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
        GridOverlayCamera.Render();
        GridOverlayCamera.clearFlags = CameraClearFlags.Nothing;

        PathOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
        PathOverlayCamera.Render();
        PathOverlayCamera.clearFlags = CameraClearFlags.Nothing;

        DisableLineViz();

    }


    protected List<List<int>> SaveAllAdjacencies()
    {
        var grid = Grid;

        List<List<int>> ret = new List<List<int>>();

        var lsize = grid.GetLength(0) * grid.GetLength(1);

        for(int i = 0; i < lsize; ++i)
        {
            ret.Add(new List<int>());
        }

        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for(int j = 0; j < grid.GetLength(1); ++j)
            {

                var cellIndex = Convert2DTo1D(i, j, grid.GetLength(0));

                ret[cellIndex] = GetAdjacencies(cellIndex);
            }
        }

        return ret;
    }

    protected int Convert2DTo1D(int x, int y, int width)
    {
        return x + y * width;
    }

    protected Vector2Int Convert1DTo2D(int i, int width)
    {
        return new Vector2Int(i % width, i / width);
    }

    protected int NeighborIndex(int nodex, int nodey, int width, TraverseDirection dir)
    {
        int xoffs = 0;
        int yoffs = 0;

        if (dir == TraverseDirection.Up || dir == TraverseDirection.UpLeft || dir == TraverseDirection.UpRight)
            yoffs = 1;
        else if (dir == TraverseDirection.Down || dir == TraverseDirection.DownLeft || dir == TraverseDirection.DownRight)
            yoffs = -1;

        if (dir == TraverseDirection.Left || dir == TraverseDirection.UpLeft || dir == TraverseDirection.DownLeft)
            xoffs = -1;
        else if (dir == TraverseDirection.Right || dir == TraverseDirection.UpRight || dir == TraverseDirection.DownRight)
            xoffs = 1;

        return Convert2DTo1D(nodex + xoffs, nodey + yoffs, width);

    }

    override public List<int> GetAdjacencies(int nodeIndex)
    {
        if (UseHardCodedCases && hardCodedCaseFound)
        {
            if (hardCodedAdjacencies != null)
            {
                return hardCodedAdjacencies[nodeIndex];
            }
            else
            {
                return new List<int>();
            }
        }


        List<int> res = new List<int>();
        var grid = Grid;

        var gridConn = GridConn;

        var width = grid.GetLength(0);
        var height = grid.GetLength(1);

        var v2 = Convert1DTo2D(nodeIndex, width);
        int nodeX = v2.x;
        int nodeY = v2.y;

        if (!grid[nodeX, nodeY])
            return res;

        foreach (var dir in (TraverseDirection[]) Enum.GetValues(typeof(TraverseDirection)))
        {
            if (gridConn == GridConnectivity.FourWay &&
                (dir == TraverseDirection.UpLeft || dir == TraverseDirection.UpRight ||
                dir == TraverseDirection.DownLeft || dir == TraverseDirection.DownRight))
                continue;



            if (CreateGrid.IsTraversable(grid, nodeX, nodeY, dir))
            {
                res.Add(NeighborIndex(nodeX, nodeY, width, dir));
            }
            
        }

        return res;
    }


    void DisableLineViz()
    {

        var linegroup = this.transform.Find(Utils.LineGroupName);

        if (linegroup != null)
        {
            linegroup.gameObject.SetActive(false);
        }
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

    //
    // Draws a square for each box of the grid that exists
    // size = This is the number of cells in the grid
    // canvas_pos = This is the where the top left corner of the canvas is
    // lengthX, lengthY = this is the width and height of the plane
    //


    void CreateGridLines(bool[,] grid, float cellSize, Vector2 canvas_pos)
    {
        //PurgeOutdatedLineViz();

        var offset = GridOverlay_OffsetFromFarCP;
        var offset2 = GridOverlay_OffsetFromFarCP - 0.01f;

        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);

        float width = grid.GetLength(0) * cellSize;
        float height = grid.GetLength(1) * cellSize;

        for (int i = 0; i <= grid.GetLength(0); i++)
        {
            Utils.DrawLine(canvas_pos + new Vector2(i * cellSize, 0f), canvas_pos + new Vector2(i * cellSize, height), offset, parent, LineColor, LineMaterial);
        }

        for (int j = 0; j <= grid.GetLength(1); j++)
        {
            Utils.DrawLine(canvas_pos + new Vector2(0f, j * cellSize), canvas_pos + new Vector2(width, j * cellSize), offset, parent, LineColor, LineMaterial);
        }

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {

                float lX = i * cellSize;
                float rX = (i + 1) * cellSize;
                float tY = j * cellSize;
                float bY = (j + 1) * cellSize;

                if (!grid[i, j])
                {
                    Utils.DrawLine(canvas_pos + new Vector2(lX, tY), canvas_pos + new Vector2(rX, bY), offset2, parent, BlockedLineColor, LineMaterial);
                    Utils.DrawLine(canvas_pos + new Vector2(rX, tY), canvas_pos + new Vector2(lX, bY), offset2, parent, BlockedLineColor, LineMaterial);
                }

            }
        }


    }



    override public int GetNodeCount()
    {
        if (Grid == null)
            return 0;

        return Grid.GetLength(0) * Grid.GetLength(1);
    }

    override public Vector2 GetNode(int nodeIndex)
    {
        if (Grid == null)
            return Vector2.zero;

        var v = Convert1DTo2D(nodeIndex, Grid.GetLength(0));

        return FindCenterOfGridCell(v.x, v.y);
    }



    //
    // Returns cell location corresponding to a particular point
    //
    public Vector2Int FindGridCellForPoint(Vector2 point)
    {
        Vector2 diff = point - BottomLeftCornerWCS;
        return new Vector2Int(Mathf.FloorToInt(diff.x / CellSize), Mathf.FloorToInt(diff.y / CellSize));
    }

    /*
     * Returns the location of the center point of a grid cell
     */
    public Vector2 FindCenterOfGridCell(int i, int j)
    {
        Vector2 local = new Vector2(i * CellSize, j * CellSize);
        local = local + new Vector2(CellSize / 2f, CellSize / 2f);
        return local + BottomLeftCornerWCS;
    }



}
