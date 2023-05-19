using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DiscretizedSpaceMonoBehavior : MonoBehaviour, IDiscretizedSpace
{

    private Obstacles obstacles;
    private List<GameObject> pathNodeMarkers = new List<GameObject>();
    private List<Vector2> pathNodes = new List<Vector2>();
    private List<List<int>> pathEdges = new List<List<int>>();

    protected Renderer Renderer;

    public Obstacles Obstacles { get => obstacles; protected set => obstacles = value; }

    public List<Vector2> PathNodes { get => pathNodes;  set => pathNodes = value; }

    public List<List<int>> PathEdges { get => pathEdges;  protected set => pathEdges = value; }

    public List<GameObject> PathNodeMarkers { get => pathNodeMarkers; protected set => pathNodeMarkers = value; }

    public Bounds Boundary { get => Renderer.bounds; }


    public Camera GridOverlayCamera;
    public Camera PathOverlayCamera;
    public Camera SearchOverlayCamera;

    public float GridOverlay_OffsetFromFarCP = 100f - 10f + 0.1f;

    public float PathOverlay_OffsetFromFarCP = 200f - 10f + 0.1f;

    public float SearchOverlay_OffsetFromFarCP = 300f - 10f + 0.1f;

    public Vector2 BottomLeftCornerWCS
    {
        get
        {
            //return new Vector2(this.transform.position.x - Boundary.size.x / 2f, this.transform.position.z - Boundary.size.z / 2f);
            return new Vector2(Boundary.min.x, Boundary.min.z);
        }
    }



    public Color PathNetworkLineColor = Color.white;
    public Color BrokenPathNetworkLineColor = Color.red;

    public Material LineMaterial;        //Material used to draw the lines for the grid

    public bool HideBlockingDetails = false; // like waypoints



    public virtual void Awake()
    {
        Renderer = GetComponent<Renderer>();

        if (Renderer == null)
            Debug.LogError("No renderer");

        //if(GridOverlayCamera == null)
        //{
        //    Debug.LogError("no grid overlay camera");
        //}

        if(PathOverlayCamera == null)
        {
            Debug.LogError("no path overlay camera");
        }

        if(SearchOverlayCamera == null)
        {
            Debug.LogError("no search overlay camera");
        }

    }


    protected void FixDumbPlaneSize()
    {  
        //planes are 10x10 instead of 1x1, which makes a mess of scaling
        //this method fixes it

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            Debug.LogError("meshFilter is null!");
        else
        {
            var mesh = meshFilter.mesh;

            const float scale = 1f / 10f;

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> scaledVerts;

            mesh.GetVertices(verts);
            scaledVerts = new List<Vector3>(verts.Count);

            foreach (var v in verts)
            {
                scaledVerts.Add(v * scale);
            }

            mesh.vertices = scaledVerts.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

        }

    }



    virtual public void Start()
    {
        FixDumbPlaneSize();
    }

    virtual public void Bake()
    {
        var obst = obstacles.getObstacles();

        foreach(var o in obst)
        {       
            o.Init();
        }
    }

    virtual public int GetNodeCount()
    {
        if (PathNodes == null)
            return 0;

        return PathNodes.Count;
    }

    virtual public Vector2 GetNode(int nodeIndex)
    {
        if (PathNodes == null)
            return Vector2.zero;

        return PathNodes[nodeIndex];
    }

    virtual public List<int> GetAdjacencies(int nodeIndex)
    {
        var edges = PathEdges[nodeIndex];

        return edges;
    }


    virtual protected void CreateNetworkLines(float offset)
    {
        //PurgeOutdatedLineViz();

        //var offset = PathOverlay_OffsetFromFarCP;

        var parent = Utils.FindOrCreateGameObjectByName(this.gameObject, Utils.LineGroupName);


        Dictionary<System.Tuple<int, int>, System.Tuple<Vector2, Vector2>> dc = new Dictionary<System.Tuple<int, int>, System.Tuple<Vector2, Vector2>>();


        //if (PathNodes != null)
        //{
            for (int i = 0; i < GetNodeCount(); ++i)
            {
                //var pts = PathEdges[i];
                var pts = GetAdjacencies(i);
                if (pts != null)
                {
                    for (int j = 0; j < pts.Count; ++j)
                    {
                        var smaller = i;
                        var bigger = pts[j];

                        if (bigger < smaller)
                        {
                            var tmp = bigger;
                            bigger = smaller;
                            smaller = tmp;
                        }

                        var tup = new System.Tuple<int, int>(smaller, bigger);
                        if (!dc.ContainsKey(tup))
                        {
                        //dc.Add(tup, new System.Tuple<Vector2, Vector2>(PathNodes[i], PathNodes[pts[j]]));
                        dc.Add(tup, new System.Tuple<Vector2, Vector2>(GetNode(i), GetNode(pts[j])));
                    }
                        else
                        {
                        //Utils.DrawLine(PathNodes[i], PathNodes[pts[j]], offset, parent, PathNetworkLineColor, LineMaterial);
                        Utils.DrawLine(GetNode(i), GetNode(pts[j]), offset, parent, PathNetworkLineColor, LineMaterial);
                        dc.Remove(tup);
                        }

                    }
                }
            }

            foreach (var key in dc.Keys)
            {
                var endpts = dc[key];
                Utils.DrawLine(endpts.Item1, endpts.Item2, offset, parent, BrokenPathNetworkLineColor, LineMaterial);
            }
        //}
    }


}
