using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using GameAICourse;

public class NavMesh : DiscretizedSpaceMonoBehavior
{
    private const string PathNodeMarkersGroupName = "PathNodeMarkersGroup";

    public MoveBall moveBall;

    List<Vector2> navmeshCentroids = new List<Vector2>();

    public Color LineColor = Color.green;
    //public Material LineMaterial;
    public Material pathEdgeMat;
    public Material polygonMat;
    List<GameObject> pathNodeObjects = new List<GameObject>();
    List<GameObject> pathNodeCentroidObjects = new List<GameObject>();

    public GameObject pathNodePrefab;
    public GameObject pathNodeOutlinePrefab;
    public GameObject pathNodeCentroidPrefab;
    public Obstacle NavmeshPolygonPrefab;

    List<Polygon> VisualizeNavMeshPolygons;

    List<Polygon> VisualizeOriginalTriangles;

    public bool ShowPathNodes = true;


    public override void Awake()
    {
        base.Awake();

        Obstacles = this.GetComponent<Obstacles>();

        if (Obstacles == null)
            Debug.LogError("no obstacles");
    }


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();


        // Following commented out because presets will immediately override...

        //Obstacles.Init();

        //Bake();

        Utils.DisplayName("CreateNavMesh", CreateNavMesh.StudentAuthorName);

    }


    public override void Bake()
    {

        base.Bake();

        List<Vector2> pnodes;
        List<List<int>> pedges;

        List<Polygon> offsetObst;
        AdjacentPolygons adjPolys;

        //Debug.Log("NavMesh: calling student code!");

        CreateNavMesh.Create(BottomLeftCornerWCS, Boundary.size.x, Boundary.size.z,
            Obstacles.GetObstaclePolygons(), moveBall.Radius,
            out offsetObst,
            out VisualizeOriginalTriangles,
            out VisualizeNavMeshPolygons,
            out adjPolys,
            out pnodes, out pedges
            );

        PathNodes = pnodes;
        PathEdges = pedges;

        CreatePathNodeMarkerObjects(PathNodes);

        PurgeOutdatedLineViz();
        CreateVizOffsetPolys(offsetObst); // TODO remove me
        CreateVizNavMesh();
        CreateVizTriangles();
        CreateNetworkLines(Utils.ZOffset);

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



    void CreateVizOffsetPolys(List<Polygon> offsetObst)
    {
        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);

        if (VisualizeNavMeshPolygons == null)
            return;

        foreach (var poly in offsetObst)
        {
            var pts = poly.getPoints();
            for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
            {
                Utils.DrawLine(pts[i], pts[j], Utils.ZOffset + 0.04f, parent, Color.blue, LineMaterial, 0.1f);
            }
        }
    }


    void CreateVizNavMesh()
    {
        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);

        if (VisualizeNavMeshPolygons == null)
            return;

        foreach(var poly in VisualizeNavMeshPolygons)
        {
            var pts = poly.getPoints();
            for (int i = 0, j=pts.Length-1; i < pts.Length; j=i++)
            {
                Utils.DrawLine(pts[i], pts[j], Utils.ZOffset, parent, Color.blue, LineMaterial, 0.006f);
            }
        }
    }

    void CreateVizTriangles()
    {
        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);


        if (VisualizeOriginalTriangles == null)
            return;

        foreach (var poly in VisualizeOriginalTriangles)
        {
            var pts = poly.getPoints();
            
            if(pts.Length == 3)
            {
                var tri = new GameObject("triangle");
                tri.transform.parent = parent.transform;
                var vt = tri.AddComponent<VisualizeTriangle>();
                vt.SetTriangle(pts[0], pts[2], pts[1]);
            }
        }
    }

    //void CreateNetworkLines()
    //{
    //    //PurgeOutdatedLineViz();

    //    var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);

    //    HashSet<System.Tuple<int, int>> hs = new HashSet<System.Tuple<int, int>>();

    //    if (PathEdges != null)
    //    {
    //        for (int i = 0; i < PathEdges.Count; ++i)
    //        {
    //            var pts = PathEdges[i];
    //            if (pts != null)
    //            {
    //                for (int j = 0; j < pts.Count; ++j)
    //                {
    //                    var smaller = i;
    //                    var bigger = pts[j];

    //                    if (bigger < smaller)
    //                    {
    //                        var tmp = bigger;
    //                        bigger = smaller;
    //                        smaller = tmp;
    //                    }

    //                    var tup = new System.Tuple<int, int>(smaller, bigger);
    //                    if (!hs.Contains(tup))
    //                    {
    //                        hs.Add(tup);
    //                        Utils.DrawLine(PathNodes[i], PathNodes[pts[j]], Utils.ZOffset, parent, LineColor, LineMaterial);
    //                    }

    //                }
    //            }
    //        }
    //    }
    //}


    void PurgeGroup(string gname)
    {
        var group = this.transform.Find(gname);

        if (group != null)
        {
            group.name = "MARKED_FOR_DELETION";
            group.gameObject.SetActive(false);
            Destroy(group.gameObject);
        }
    }

    void PurgeOutdatedPathNodeMarkers()
    {
        PurgeGroup(PathNodeMarkersGroupName);
    }


    void CreatePathNodeMarkerObjects(List<Vector2> pathNodes)
    {
        PurgeOutdatedPathNodeMarkers();

        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, PathNodeMarkersGroupName);


        GameObject prefab = pathNodePrefab;

        if (HideBlockingDetails)
        {
            prefab = pathNodeOutlinePrefab;
        }



        if (ShowPathNodes)
        {
            foreach (Vector2 pn in pathNodes)
            {
                GameObject pno = Instantiate(prefab, new Vector3(pn.x, Utils.ZOffset + 0.01f, pn.y), Quaternion.identity, parent.transform);
                pno.transform.localScale = Vector3.one * 2f * moveBall.Radius;
                pathNodeObjects.Add(pno);
            }
        }
    }

}
