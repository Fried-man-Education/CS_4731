using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using GameAICourse;

public class MoveBall : MonoBehaviour, IBallMover
{

    private const string PathVizGroupName = "PathViz";

    private const string CurrentNodeGroupName = "CurrentNodeParent";

    public float Speed = 1.0f;

    public bool HideSearchViz = false;

    public float IncrementalSearchTimeInterval = 0.0f;
    protected float LastSearchTime = 0f;


    Rigidbody rigidBody;
    SphereCollider sphereCollider;
 
    public LayerMask mask;


    public GameObject PathBeginPrefab;
    public GameObject PathEndPrefab;
    public GameObject PathOpenPrefab;
    public GameObject PathClosedPrefab;
    public GameObject PathOpenEmptyPrefab;
    public GameObject PathClosedEmptyPrefab;
    public GameObject PathEndWithArrowPrefab;
    public GameObject PathCurrentPrefab;

    DiscretizedSpaceMonoBehavior discretizedSpace;
    GameGrid gridSpace;

    //IDiscretizedSpace discretizedSpace;

    //the distance at which the path node is reached
    float epsilon = 0.01f;

    List<int> followPath;


    public float Radius { get => this.sphereCollider.radius * this.transform.lossyScale.x; }


    protected float MarkerSize
    {
        get
        {
            if (gridSpace != null)
                return 0.8f * gridSpace.CellSize;
            else
                return 2f * Radius;
        }
    }

    public Vector2 CurrentPosition
    {
        get => new Vector2(transform.position.x, transform.position.z);

        set
        {
            rigidBody.MovePosition(new Vector3(value.x, 0f, value.y));

            Init();
        }
    }


    public Color StartMarkerColor = Color.green;
    public Color EndMarkerColor = Color.blue;
    public Color PathColor = Color.gray;
    public Material MarkerLineMaterial;
    public int IncrSearchMaxNodeExplore = 1;
    public bool UsePathRefinement = false;


    List<Tuple<Vector2, Vector2>> rawPath;
    int currPathIndex = 0;
    float pathStartTime;

    bool initSearch = false;
    bool activeSearch = false;
    bool incrementalSearch = false;
    bool stepSearch = false;
    int nearestGoalNodeIndex = -1;
    int nearestStartNodeIndex = -1;
    //Vector2 startPos;
    Vector2 goalPos;
    int currentNode = -1;
    Dictionary<int, PathSearchNodeRecord> searchNodeRecords = null;
    Priority_Queue.SimplePriorityQueue<int, float> openNodes = null;
    HashSet<int> closedNodes = null;
    List<int> returnPath = null;

    // These are used for bookkeeping of incremental searches for efficient
    // drawing of markers
    HashSet<int> prevClosedNodes = new HashSet<int>();
    HashSet<int> prevOpenNodes = new HashSet<int>();


    void Awake()
    {
        rigidBody = this.GetComponent<Rigidbody>();

        if (!rigidBody)
            Debug.LogError("No rigid body");

        sphereCollider = this.GetComponent<SphereCollider>();

        if (!sphereCollider)
            Debug.LogError("No collider");
    }

    void Start()
    {

        //discretizedSpace = (IDiscretizedSpace)Manager.Instance.DiscretizedSpace;
        discretizedSpace = Manager.Instance.DiscretizedSpace;

        if (discretizedSpace == null)
            Debug.LogError("path network is null");

        gridSpace = discretizedSpace as GameGrid;


        if(PathBeginPrefab == null)
        {
            Debug.LogError("PathBeginPrefab is null");
        }

        if(PathEndPrefab == null)
        {
            Debug.LogError("PathEndPrefab is null");
        }

        if(PathOpenPrefab == null)
        {
            Debug.LogError("PathOpenPrefab is null");
        }

        if(PathClosedPrefab == null)
        {
            Debug.LogError("PathClosedPrefab is null");
        }

        if (PathClosedEmptyPrefab == null)
        {
            Debug.LogError("PathClosedEmptyPrefab is null");
        }

        if (PathOpenEmptyPrefab == null)
        {
            Debug.LogError("PathOpenEmptyPrefab is null");
        }

        if (PathEndWithArrowPrefab == null)
        {
            Debug.LogError("PathEndWithArrowPrefab is null");
        }

        if(PathCurrentPrefab == null)
        {
            Debug.LogError("PathCurrentPrefab is null");
        }

        Init();
    }

    public void Init()
    {
        rawPath = null;
        currPathIndex = 0;

        PurgeOutdatedLineViz();
        PurgeCurrentNodeViz();

        if (this.discretizedSpace == null)
        {
            this.discretizedSpace = Manager.Instance.DiscretizedSpace;
            gridSpace = discretizedSpace as GameGrid;
        }

        if (this.discretizedSpace != null)
        {

            this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
            this.discretizedSpace.SearchOverlayCamera.Render();
            this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Nothing;
        }

    }


    PathSearchAlgorithms prevAlg = PathSearchAlgorithms.BreadthFirstSearch;

    

    // Update is called once per frame
    void Update()
    {

        PathSearchResultType pathResult = PathSearchResultType.InProgress;

        bool searchUpdated = false;


        bool nextSearchIncr = false;


        // if step search, go next incr
        if(Input.GetKeyUp(KeyCode.N))
        {
            nextSearchIncr = true;
        }


        if (activeSearch)
        {
            ++DEBUG_activeCount;


            if (Manager.Instance.PathSearchAlgorithm != prevAlg)
            {

                resetSearch();
                PurgeCurrentNodeViz();
                PurgeOutdatedLineViz();
                this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
                this.discretizedSpace.SearchOverlayCamera.Render();
                this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Nothing;

                prevAlg = Manager.Instance.PathSearchAlgorithm;
            }
            else
            {

                bool useManhattan = gridSpace != null && gridSpace.GridConn == GridConnectivity.FourWay;

                PathSearchVariantType variant = useManhattan ? PathSearchVariantType.ManhattanDistance : default;



                if (incrementalSearch)
                {
                    if ((stepSearch && nextSearchIncr) ||
                        (!stepSearch &&
                            Time.timeSinceLevelLoad > (LastSearchTime + IncrementalSearchTimeInterval)))
                    {
                        LastSearchTime = Time.timeSinceLevelLoad;
                     
                        pathResult = Manager.Instance.PathSearchProvider.FindPathIncremental(
                            //discretizedSpace.PathNodes,
                            //discretizedSpace.PathEdges,
                            discretizedSpace.GetNodeCount,
                            discretizedSpace.GetNode,
                            discretizedSpace.GetAdjacencies,
                            variant, nearestStartNodeIndex, nearestGoalNodeIndex,
                            IncrSearchMaxNodeExplore, initSearch, ref currentNode, ref searchNodeRecords, ref openNodes,
                            ref closedNodes, ref returnPath);

                        searchUpdated = true;
                    }
                }
                else
                {
                    pathResult = Manager.Instance.PathSearchProvider.FindPath(
                        //discretizedSpace.PathNodes,
                        //discretizedSpace.PathEdges,
                        discretizedSpace.GetNodeCount,
                        discretizedSpace.GetNode,
                        discretizedSpace.GetAdjacencies,
                        variant, nearestStartNodeIndex, nearestGoalNodeIndex,
                        ref currentNode, ref searchNodeRecords, ref openNodes, ref closedNodes, ref returnPath);

                    searchUpdated = true;
                }
            }
        }


        if (searchUpdated)
        {

            DEBUG_pathResult = pathResult;
            DEBUG_startIndex = nearestStartNodeIndex;
            DEBUG_goalIndex = nearestGoalNodeIndex;

            var pNodeCount = discretizedSpace.GetNodeCount();

            if (nearestStartNodeIndex >= 0 && nearestStartNodeIndex < pNodeCount)
                DEBUG_edgesAtStart = discretizedSpace.GetAdjacencies(nearestStartNodeIndex);
            if (nearestGoalNodeIndex >= 0 && nearestGoalNodeIndex < pNodeCount)
                DEBUG_edgesAtGoal = discretizedSpace.GetAdjacencies(nearestGoalNodeIndex);

            if (initSearch)
            {
                //Debug.Log("Search was initialized");

                initSearch = false;
            }


            if (activeSearch)
            {


                //Debug.Log("Updating search viz");

                // TODO with overlay, can probably avoid purging and completely recreating every time.
                // Especially if switch to circle markers and directional arrows

                PurgeOutdatedLineViz();

                //CreateMarkerLines(discretizedSpace.PathNodes[nearestGoalNodeIndex], 0.2f, EndMarkerColor);

                //CreateMarkerLines(discretizedSpace.PathNodes[nearestStartNodeIndex], 0.2f, StartMarkerColor);

                CreateEndMarker(discretizedSpace.GetNode(nearestGoalNodeIndex), MarkerSize);
                CreateStartMarker(discretizedSpace.GetNode(nearestStartNodeIndex), MarkerSize);

                //Debug.Log("goal marker should be at: " + discretizedSpace.PathNodes[nearestGoalNodeIndex]);


                if(!HideSearchViz)
                    CreateSetMarkers(discretizedSpace.GetNodeCount, discretizedSpace.GetNode, searchNodeRecords, currentNode, openNodes, closedNodes, prevOpenNodes, prevClosedNodes);


                // Track previous nodes for an incremental search. Allows for efficient incremental draw
                prevOpenNodes.Clear();
                prevClosedNodes.Clear();

                foreach (var n in openNodes)
                {
                    if(n != nearestGoalNodeIndex) //always want to redraw the goal for orientation
                        prevOpenNodes.Add(n);
                }

                foreach (var n in closedNodes)
                {
                    if(n != nearestGoalNodeIndex) //always want to redraw the goal for orientation
                        prevClosedNodes.Add(n);
                }


                // Check if path complete
                if (pathResult != PathSearchResultType.InProgress)
                {
                    //Debug.Log("Ending active search because pathResult was not InProgress: " + pathResult);

                    activeSearch = false;

                    if (pathResult != PathSearchResultType.InitializationError)
                    {


                        //rawPath = GenerateRawPath(returnPath, CurrentPosition, goalPos, pathResult);

                        var fullPath = GenerateFullPath(returnPath, CurrentPosition, goalPos, pathResult);

                        List<Vector2> refinedPath = null;

                        bool refPathRes = false;

                        if (UsePathRefinement)
                        {
                            Debug.Log("Path refinement enabled!");

                            refPathRes = PathRefinement.Refine(
                                new Vector2(discretizedSpace.Boundary.min.x, discretizedSpace.Boundary.min.z),
                                discretizedSpace.Boundary.size.x,
                                discretizedSpace.Boundary.size.z,
                                Radius,
                                fullPath,
                                discretizedSpace.Obstacles.getObstacles(),
                                1,
                                0,
                                out refinedPath);
                        }

                        DEBUG_path = returnPath;
                        DEBUG_rawPath = rawPath;

                        if (refPathRes)
                            fullPath = refinedPath;

                        rawPath = GenerateRawPath(fullPath);

                        currPathIndex = 0;
                        pathStartTime = Time.timeSinceLevelLoad;

                        CreatePathLines(fullPath);//, CurrentPosition, goalPos, pathResult);


                    }
                }

                discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Depth;
                discretizedSpace.SearchOverlayCamera.Render();
                discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Nothing;


                //PurgeOutdatedLineViz();
            }

        }

        // move ball along path
        if (rawPath != null && rawPath.Count > 0 && currPathIndex < rawPath.Count)
        {
            var currPathSegment = rawPath[currPathIndex];
            var currDist = Vector2.Distance(currPathSegment.Item1, currPathSegment.Item2);
            var currTime = Time.timeSinceLevelLoad;
            var elapsedTime = currTime - pathStartTime;
            var expectedArrivalTime = currDist / Speed;
            var distTraveled = elapsedTime * Speed;

            if( elapsedTime >= expectedArrivalTime)
            {
                ++currPathIndex;
                var pos = new Vector3(currPathSegment.Item2.x, 0f, currPathSegment.Item2.y);
                rigidBody.MovePosition(pos);
                pathStartTime = Time.timeSinceLevelLoad;
            }
            else
            {
                var t = 1f;
                if (currDist >= epsilon)
                    t = distTraveled / currDist;

                var newPos = Vector2.Lerp(currPathSegment.Item1, currPathSegment.Item2, t);
                var pos = new Vector3(newPos.x, 0f, newPos.y);
                rigidBody.MovePosition(pos);
            }

        }


        //clear the current search
        if(Input.GetKeyUp(KeyCode.C))
        {
            activeSearch = false;
            rawPath = null;
            PurgeOutdatedLineViz();
            PurgeCurrentNodeViz();
            this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
            this.discretizedSpace.SearchOverlayCamera.Render();
            this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Nothing;
        }

        //toggle hidden search metadata
        if(Input.GetKeyUp(KeyCode.H))
        {

            HideSearchViz = !HideSearchViz;
        }

    }


    protected Vector2Int Convert1DTo2D(int i, int width)
    {
        return new Vector2Int(i % width, i / width);
    }

    int NearestValidNodeIndex(Vector2 v)
    {
        var vint = CG.Convert(v);

        var minDist = float.MaxValue;
        int minNodeIndex = -1;

        var almostGoodMinDist = float.MaxValue;
        int almostGoodMinNodeIndex = -1;

        var min = discretizedSpace.Boundary.min;
        var max = discretizedSpace.Boundary.max;
       
        //var origin = discretizedSpace.BottomLeftCornerWCS;

        // Needs to be counterclockwise!
        Vector2[] polyPts =
            {
                   new Vector2(min.x, min.z),
                   new Vector2(max.x, min.z),
                   new Vector2(max.x, max.z),
                   new Vector2(min.x, max.z)
             };


        var gameGrid = discretizedSpace as GameGrid;

        var pNodeCount = discretizedSpace.GetNodeCount();

        for (int i = 0; i < pNodeCount; ++i)
        {
            var n = discretizedSpace.GetNode(i);

            var nint = CG.Convert(n);

            var dist = Vector2.Distance(v, n);


            if (dist < minDist)
            {

                bool goodPoint = true;


                if (CG.InPoly1(polyPts, n) == CG.PointPolygonIntersectionType.Outside)
                {
                    //Debug.Log("Node can't be outside boundary");
                    goodPoint = false;
                    continue;
                }

                if (gameGrid != null)
                {
                    var width = gameGrid.Grid.GetLength(0);

                    var gridCoords = Convert1DTo2D(i, width);

                    if (!gameGrid.Grid[gridCoords.x, gridCoords.y])
                    {
                        goodPoint = false;
                        continue;
                    }
                }
                else
                {

                    foreach (var o in discretizedSpace.Obstacles.getObstacles())
                    {
                        if (o.IsPointInPolygon(n))
                        {
                            //Debug.Log("Point in polygon");
                            goodPoint = false;
                            break;
                        }
                        //else if(Utils.Intersects(v,n, o.GetPolygon()))
                        else if (CG.IntersectionLineSegmentWithPolygon(vint, nint, o.GetIntegerPoints()))
                        {
                            //Debug.Log("No line of sight");
                            goodPoint = false;
                            break;
                        }
                    }

                    if (!goodPoint)
                        continue;

                    var edges = discretizedSpace.PathEdges[i];

                    if (edges.Count <= 0)
                    {
                        if (dist < almostGoodMinDist)
                        {
                            almostGoodMinDist = dist;
                            almostGoodMinNodeIndex = i;
                        }
                        continue;
                    }
                }

                minDist = dist;
                minNodeIndex = i;
            }

        }

        if(minNodeIndex <= -1)
        {
            if(almostGoodMinNodeIndex > -1)
            {
                minNodeIndex = almostGoodMinNodeIndex;
            }
        }

        return minNodeIndex;
    }


    public int DEBUG_activeCount = 0;
    public List<int> DEBUG_path;
    public List<Tuple<Vector2, Vector2>> DEBUG_rawPath;
    public PathSearchResultType DEBUG_pathResult;
    public int DEBUG_startIndex;
    public int DEBUG_goalIndex;
    public List<int> DEBUG_edgesAtGoal;
    public List<int> DEBUG_edgesAtStart;



    static bool ballTooClose(Vector2 testPos, Vector2 lineposA, Vector2 lineposB, float ballRadius)
    {
        //var d = Utils.DistanceToLine(testPos, lineposA, lineposB);

        var d = CG.DistanceToLineSegment(testPos, lineposA, lineposB);

        return (d <= ballRadius);
        
    }

    public void OnClicked(RaycastHit hit, bool isLeft)
    {
        PurgeOutdatedLineViz();

        var clickLoc = new Vector2(hit.point.x, hit.point.z);


        var min = discretizedSpace.Boundary.min;
        var max = discretizedSpace.Boundary.max;

        //var origin = discretizedSpace.BottomLeftCornerWCS;

        // Needs to be counterclockwise!
        Vector2[] polyPts =
            {
                   new Vector2(min.x, min.z),
                   new Vector2(max.x, min.z),
                   new Vector2(max.x, max.z),
                   new Vector2(min.x, max.z)
             };

        if (CG.InPoly1(polyPts, clickLoc) == CG.PointPolygonIntersectionType.Outside)
        {
            Debug.Log("Ball can't go outside boundary");
            return;
        }


        if (
            ballTooClose(clickLoc, new Vector2(min.x, min.z), new Vector2(max.x, min.z), Radius) ||
            ballTooClose(clickLoc, new Vector2(max.x, min.z), new Vector2(max.x, max.z), Radius) ||
            ballTooClose(clickLoc, new Vector2(max.x, max.z), new Vector2(min.x, max.z), Radius) ||
            ballTooClose(clickLoc, new Vector2(min.x, max.z), new Vector2(min.x, min.z), Radius))
        {
            Debug.Log("Ball can't fit here");
            return;
        }


        if (gridSpace != null)
        {
            var gridCell = gridSpace.FindGridCellForPoint(clickLoc);

            if(gridCell.x >= 0 && gridCell.x < gridSpace.Grid.GetLength(0) &&
                gridCell.y >= 0 && gridCell.y < gridSpace.Grid.GetLength(1))
            {
                if(!gridSpace.Grid[gridCell.x, gridCell.y])
                {
                    Debug.Log("Can't move to this grid cell");
                    return;
                }
            }
        }
        else
        {

            foreach (var o in discretizedSpace.Obstacles.getObstacles())
            {
                //cannot click inside a shape since those can be dragged and catch the mouse
                //but if that changes...

                var pts = o.GetPoints();
                for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
                {
                    var pt0 = pts[i];
                    var pt1 = pts[j];

                    if (ballTooClose(clickLoc, pt0, pt1, Radius))
                    {
                        Debug.Log("Ball can't fit here");
                        return;
                    }
                }

            }
        }

        this.nearestGoalNodeIndex = NearestValidNodeIndex(clickLoc);

        if(this.nearestGoalNodeIndex < 0)
        {
            Debug.Log("Not a valid click location");
            return;
        }

        var nearestGoalNode = discretizedSpace.GetNode(this.nearestGoalNodeIndex);

        this.nearestStartNodeIndex = NearestValidNodeIndex(CurrentPosition);

        if(this.nearestStartNodeIndex < 0)
        {
            Debug.Log("Could not assign start location");
            return;
        }

        var nearestStartNode = discretizedSpace.GetNode(this.nearestStartNodeIndex);

        //startPos = CurrentPosition;
        this.goalPos = clickLoc;


        // Indicates the new search to happen
        initSearch = true;
        activeSearch = true;
        incrementalSearch = !isLeft;
        stepSearch = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        rawPath = null;
        currPathIndex = 0;
        currentNode = nearestStartNodeIndex;
        prevOpenNodes.Clear();
        prevClosedNodes.Clear();
        LastSearchTime = Time.timeSinceLevelLoad;
        prevAlg = Manager.Instance.PathSearchAlgorithm;


        CreateEndMarker(discretizedSpace.GetNode(nearestGoalNodeIndex), MarkerSize);
        CreateStartMarker(discretizedSpace.GetNode(nearestStartNodeIndex), MarkerSize);
        CreateCurrentOverlayMarker(nearestStartNode, MarkerSize, 0.08f);

        this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.SolidColor;
        this.discretizedSpace.SearchOverlayCamera.Render();
        this.discretizedSpace.SearchOverlayCamera.clearFlags = CameraClearFlags.Nothing;


    }


    void resetSearch()
    {

        rawPath = null;
        currPathIndex = 0;
        currentNode = -1;
        prevOpenNodes.Clear();
        prevClosedNodes.Clear();
        incrementalSearch = false;
        activeSearch = false;

    }




    List<Vector2> GenerateFullPath(List<int> path, Vector2 startPos, Vector2 goalPos, PathSearchResultType pathRes)
    {

        if (path == null || path.Count < 1)
            return null;

        var resPath = new List<Vector2>(path.Count + 2);

        resPath.Add(startPos);
        
        for (int i = 0; i < path.Count; ++i)
        {
            resPath.Add(discretizedSpace.GetNode(path[i]));
        }

        if (pathRes == PathSearchResultType.Complete)
        {
            resPath.Add(goalPos);
        }

        return resPath;

    }

    List<System.Tuple<Vector2, Vector2>> GenerateRawPath(List<Vector2> path)
    {
        var res = new List<System.Tuple<Vector2, Vector2>>(path.Count);

        if (path.Count < 2)
            return res;

        for (int i = 0; i < path.Count - 1; ++i)
            res.Add(new Tuple<Vector2, Vector2>(path[i], path[i + 1]));


        return res;
    }

    List<System.Tuple<Vector2, Vector2>> GenerateRawPath(List<int> path, Vector2 startPos, Vector2 goalPos, PathSearchResultType pathRes)
    {

        if (path == null || path.Count < 1)
            return null;

        var rawPath = new List<System.Tuple<Vector2, Vector2>>(path.Count + 2);

        rawPath.Add(new System.Tuple<Vector2, Vector2>(startPos, discretizedSpace.GetNode(path[0])));

        for(int i = 0; i<path.Count -1; ++i)
        {
            rawPath.Add(new System.Tuple<Vector2, Vector2>(discretizedSpace.GetNode(path[i]), discretizedSpace.GetNode(path[i + 1])));

        }

        if(pathRes == PathSearchResultType.Complete)
        {
            rawPath.Add(new System.Tuple<Vector2, Vector2>(discretizedSpace.GetNode(path[path.Count-1]), goalPos));
        }

        return rawPath;

    }

    void PurgeCurrentNodeViz()
    {
        //var linegroup = this.transform.Find(PathVizGroupName);
        var group = GameObject.Find("/" + CurrentNodeGroupName);

        if (group != null)
        {
            group.name = "MARKED_FOR_DELETION";
            group.gameObject.SetActive(false);
            Destroy(group.gameObject);
        }
    }

    void PurgeOutdatedLineViz()
    {

        //var linegroup = this.transform.Find(PathVizGroupName);
        var linegroup = GameObject.Find("/" + PathVizGroupName);

        if (linegroup != null)
        {
            linegroup.name = "MARKED_FOR_DELETION";
            linegroup.gameObject.SetActive(false);
            Destroy(linegroup.gameObject);
        }
    }


    void CreateSetMarkers(
        //List<Vector2> nodes,
        GetNodeCount getNodeCount,
        GetNode getNode,
        Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
        int currNode,
        Priority_Queue.SimplePriorityQueue<int, float> openNodes,
        HashSet<int> closedNodes,
        HashSet<int> prevOpenNodes,
        HashSet<int> prevClosedNodes
        )
    {

        float overlayOffset = 0.08f;

        //Debug.Log($"Current node is: {currNode}");

        foreach (var nindex in openNodes)
        {
            var node = getNode(nindex);

            if (nindex == currNode)
            {
                //Debug.Log($"Current node drawn: {currNode}");
                // NOT part of the overlay texture!
                CreateCurrentOverlayMarker(node, MarkerSize, overlayOffset);
            }

            if (prevOpenNodes.Contains(nindex))
                continue;

            var pindex = searchNodeRecords[nindex].FromNodeIndex;

            float angle = 0f;
            bool directed = false;

            var nodeCount = getNodeCount();

            if (pindex >= 0 && pindex < nodeCount)
            {
                var pnode = getNode(pindex);
                Vector2 flip = new Vector2(1f, -1f);
                angle = Vector2.SignedAngle(Vector2.right, flip * (pnode - node));
                directed = true;
            }

            //Debug.Log("Angle: " + angle);

            if (nindex != this.nearestStartNodeIndex)
            {
                if (directed)
                {
                    //CreateDirectedMarkerLines(node, 0.1f, Color.white, angle);


                    if (nindex == this.nearestGoalNodeIndex)
                    {
                        CreateEndDirectedMarker(node, MarkerSize, angle);
                    }
                    else
                    {
                        CreateOpenDirectedMarker(node, MarkerSize, angle);
                    }

                }
                else
                {
                    //CreateMarkerLines(node, 0.1f, Color.white, 0.01f);
                    CreateOpenEmptyMarker(node, MarkerSize);
                }
            }


        }

        foreach(var nindex in closedNodes)
        {
            var node = getNode(nindex);

            if (nindex == currNode)
            {
                //Debug.Log($"Current node drawn: {currNode}");
                // NOT part of the overlay texture!
                CreateCurrentOverlayMarker(node, MarkerSize, overlayOffset);
            }

            if (prevClosedNodes.Contains(nindex))
                continue;

            var pindex = searchNodeRecords[nindex].FromNodeIndex;

            float angle = 0f;
            bool directed = false;

            var nodeCount = getNodeCount();

            if (pindex >= 0 && pindex < nodeCount)
            {
                var pnode = getNode(pindex);
                Vector2 flip = new Vector2(1f, -1f);
                angle = Vector2.SignedAngle(Vector2.right, flip*(pnode - node));
                directed = true;
            }

            if (nindex != this.nearestStartNodeIndex)
            {
                if (directed)
                {
                    //CreateDirectedMarkerLines(node, 0.1f, Color.cyan, angle);


                    if (nindex == this.nearestGoalNodeIndex)
                    {
                        CreateEndDirectedMarker(node, MarkerSize, angle);
                    }
                    else
                    {
                        CreateClosedDirectedMarker(node, MarkerSize, angle);
                    }

                }
                else
                {
                    //CreateMarkerLines(node, 0.1f, Color.white, 0.01f);
                    CreateClosedEmptyMarker(node, MarkerSize);
                }
            }

        }

    }


    void CreatePathLines(List<Vector2> path) //List<int> path, Vector2 startPos, Vector2 goalPos, PathSearchResultType pathResult)
    {

        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.3f; //Utils.ZOffset * 1.9f

        //var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, PathVizGroupName);
        var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);

        if (path.Count < 1)
            return;

        //Utils.DrawLine(startPos, discretizedSpace.GetNode(path[0]), offset, parent, PathColor, MarkerLineMaterial);

        for(int i=0; i<path.Count-1; ++i)
        {
            var pn1 = path[i];
            var pn2 = path[i + 1];

            //Utils.DrawLine(discretizedSpace.GetNode(pn1), discretizedSpace.GetNode(pn2), offset, parent, PathColor, MarkerLineMaterial);
            Utils.DrawLine(pn1, pn2, offset, parent, PathColor, MarkerLineMaterial);
        }

        //if(pathResult == PathSearchResultType.Complete)
        //{
        //    Utils.DrawLine(discretizedSpace.GetNode(path[path.Count-1]), goalPos, offset, parent, PathColor, MarkerLineMaterial);
        //}

    }

    void CreateOpenDirectedMarker(Vector2 pos, float halfWidth, float angleRot)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f;
        CreateDirectedMarker(pos, halfWidth, angleRot, offset, PathOpenPrefab);
    }

    void CreateClosedDirectedMarker(Vector2 pos, float halfWidth, float angleRot)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f;
        CreateDirectedMarker(pos, halfWidth, angleRot, offset, PathClosedPrefab);
    }


    void CreateEndDirectedMarker(Vector2 pos, float halfWidth, float angleRot)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.22f;
        CreateDirectedMarker(pos, halfWidth, angleRot, offset, PathEndWithArrowPrefab);
    }


    void CreateDirectedMarker(Vector2 pos, float halfWidth, float angleRot, float offset, GameObject markerPrefab)
    {

        //var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f; //Utils.ZOffset * 2f

        var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);

        var pos3d = new Vector3(pos.x, offset, pos.y);

        var newMarker = Instantiate(markerPrefab, parent.transform, true);
        newMarker.transform.position = pos3d;
        newMarker.transform.rotation = Quaternion.Euler(0f, angleRot, 0f);
        newMarker.transform.localScale = Vector3.one * halfWidth;
    }

    void CreateDirectedMarkerLines(Vector2 pos, float halfWidth, Color c, float angleRot)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f; //Utils.ZOffset * 2f

        float lineWidth = 0.01f;
        //var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, PathVizGroupName);
        var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);

        var rot = Quaternion.Euler(0f, angleRot, 0f);

        Vector3 pos3d, p1, p2;
        pos3d = new Vector3(pos.x, 0f, pos.y);

        p1 = new Vector3(halfWidth, 0f, 0f);
        p2 = new Vector3(0, 0f, -halfWidth / 2f);

        p1 = pos3d + rot * p1;
        p2 = pos3d + rot * p2;

        Utils.DrawLine(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z),
            offset, parent, c, MarkerLineMaterial, lineWidth);

        p1 = new Vector3(0f,0f, -halfWidth / 2f);
        p2 = new Vector3(0f, 0f, halfWidth / 2f);

        p1 = pos3d + rot * p1;
        p2 = pos3d + rot * p2;

        Utils.DrawLine(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z),
            offset, parent, c, MarkerLineMaterial, lineWidth);


        p1 = new Vector3(0f,0f, halfWidth / 2f);
        p2 = new Vector3(halfWidth, 0f, 0f);

        p1 = pos3d + rot * p1;
        p2 = pos3d + rot * p2;

        Utils.DrawLine(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z),
            offset, parent, c, MarkerLineMaterial, lineWidth);
      
    }


    void CreateEndMarker(Vector2 pos, float halfWidth)
    {
        //Debug.Log("end marker is being placed at: " + pos);

        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.21f;
        CreateMarker(pos, halfWidth, offset, this.PathEndPrefab);
    }

    void CreateStartMarker(Vector2 pos, float halfWidth)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.22f;
        CreateMarker(pos, halfWidth, offset, this.PathBeginPrefab);
    }


    void CreateOpenEmptyMarker(Vector2 pos, float halfWidth)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f;
        CreateMarker(pos, halfWidth, offset, this.PathOpenEmptyPrefab);
    }

    void CreateClosedEmptyMarker(Vector2 pos, float halfWidth)
    {
        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f;
        CreateMarker(pos, halfWidth, offset, this.PathClosedEmptyPrefab);
    }


    // This will no doubt cause confusion for me in the future. Most of the icons
    // are rendered to the overlay. But the "current" overlay is a game object seen
    // by the main camera




    protected GameObject currentMarker = null;
    GameObject CreateCurrentOverlayMarker(Vector2 pos, float halfWidth, float offset)
    {
        //var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.23f;

        var parent = Utils.FindOrCreateGameObjectByName(null, CurrentNodeGroupName);

        if (currentMarker == null)
            currentMarker = CreateMarker(pos, halfWidth, offset, this.PathCurrentPrefab, parent);
        else
        {
            currentMarker.transform.position = new Vector3(pos.x, offset, pos.y);
            currentMarker.transform.localScale = Vector3.one * halfWidth;
        }

        return currentMarker;
    }

    GameObject CreateMarker(Vector2 pos, float halfWidth, float offset, GameObject markerPrefab)
    {
        var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);
        return CreateMarker(pos, halfWidth, offset, markerPrefab, parent);
    }


    GameObject CreateMarker(Vector2 pos, float halfWidth, float offset, GameObject markerPrefab, GameObject parent)
    {
        //var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f; //Utils.ZOffset * 2f

        //var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, PathVizGroupName);
        //var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);

        var pos3d = new Vector3(pos.x, offset, pos.y);

        var newMarker = Instantiate(markerPrefab, parent.transform, true);
        newMarker.transform.position = pos3d;
        newMarker.transform.localScale = Vector3.one * halfWidth;

        //Debug.Log($"{newMarker.name} is positioned at: {newMarker.transform.position}");

        return newMarker;
    }


    void CreateMarkerLines(Vector2 pos, float halfWidth, Color c, float lineWidth = 0.04f)
    {

        var offset = discretizedSpace.SearchOverlay_OffsetFromFarCP + 0.2f; //Utils.ZOffset * 2f

        //var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, PathVizGroupName);
        var parent = Utils.FindOrCreateGameObjectByName(null, PathVizGroupName);

        Utils.DrawLine(pos + new Vector2(halfWidth, halfWidth), pos + new Vector2(halfWidth, -halfWidth),
            offset, parent, c, MarkerLineMaterial, lineWidth);

        Utils.DrawLine(pos + new Vector2(halfWidth, -halfWidth), pos + new Vector2(-halfWidth, -halfWidth),
            offset, parent, c, MarkerLineMaterial, lineWidth);

        Utils.DrawLine(pos + new Vector2(-halfWidth, -halfWidth), pos + new Vector2(-halfWidth, halfWidth),
            offset, parent, c, MarkerLineMaterial, lineWidth);

        Utils.DrawLine(pos + new Vector2(-halfWidth, halfWidth), pos + new Vector2(halfWidth, halfWidth),
            offset, parent, c, MarkerLineMaterial, lineWidth);

    }


}
